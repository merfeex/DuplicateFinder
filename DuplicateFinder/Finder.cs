using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace DuplicateFinder
{
    public static class Finder
    {
        public static IDictionary<string, IList<string>> FindDuplicates(params string[] paths)
        {
            var dirs = new Queue<string>();
            foreach(var path in paths)
            {
                if (!File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                {
                    throw new ArgumentException("Not a directory: " + path);
                }
                if (!dirs.Contains(path))
                {
                    dirs.Enqueue(path);
                }
                GetDirectories(dirs, path);
            }

            Task[] tasks = new Task[dirs.Count];            
            var dict = new ConcurrentDictionary<string, IList<string>>();
            int cnt = 0;
            while(dirs.Count > 0)
            {
                string dir = dirs.Dequeue();
                tasks[cnt] = Task.Run(() => HandleFiles(dir, dict));
                cnt++;
            }
            Task.WaitAll(tasks);

            Dictionary<string, IList<string>> retValue = new Dictionary<string, IList<string>>();
            foreach (var kvp in dict)
            {
                if (kvp.Value.Count > 1)
                {
                    retValue[kvp.Key] = kvp.Value;
                }
            }
            return retValue;
        }

        private static void GetDirectories(Queue<string> queue, string directory)
        {
            string[] dirs;
            try
            {
                dirs = Directory.GetDirectories(directory);
            }
            catch (Exception e)
            {
                if ((e is IOException) || (e is UnauthorizedAccessException))
                {
                    return;
                }
                else
                {
                    throw e;
                }
            }
            foreach (var d in dirs)
            {
                if (!queue.Contains(d))
                {
                    queue.Enqueue(d);
                }
                GetDirectories(queue, d);
            }
        }

        private static void HandleFiles(string path, ConcurrentDictionary<string, IList<string>> dict)
        {
            string[] files;
            try
            {
                files = Directory.GetFiles(path);
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }
            for (int i = 0; i < files.Length; i++)
            {
                try
                {
                    string hash = GetHash(files[i]);
                    if (hash != string.Empty)
                    {
                        dict.AddOrUpdate(hash, new List<string> { files[i] }, (k, v) => { v.Add(files[i]); return v; });
                    }
                }
                catch (IOException) { }
            }
        }

        private static string GetHash(string fileName)
        {
            using (MD5 hasher = MD5.Create())
            {
                try
                {
                    using (Stream stream = File.OpenRead(fileName))
                    {
                        byte[] hash = hasher.ComputeHash(stream);
                        return BitConverter.ToString(hash).Replace("-", "").ToLower();
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    return string.Empty;
                }
            }
        }
    }
}
