using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Formatters;
using System.Security.Cryptography;
using System.Threading;
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
            int cnt = dirs.Count;
            for (int i = 0; i < cnt; i++) {
                tasks[i] = Task.Run(() => HandleFiles(dirs.Dequeue(), dict));
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
            try
            {
                var dirs = Directory.GetDirectories(directory);
                foreach(var d in dirs)
                {
                    if (!queue.Contains(d))
                    {
                        queue.Enqueue(d);
                    }
                    GetDirectories(queue, d);
                }
            }
            catch (IOException)
            {
                //skip directory
            }
        }

        private static void HandleFiles(string path, ConcurrentDictionary<string, IList<string>> dict)
        {
            string[] files = Directory.GetFiles(path);
            for (int i = 0; i < files.Length; i++)
            {
                try
                {
                    string hash = GetHash(files[i]);
                    dict.AddOrUpdate(hash, new List<string> { files[i] }, (k, v) => { v.Add(files[i]); return v; });
                }
                catch (IOException) { }
            }
        }

        private static string GetHash(string fileName)
        {
            using (MD5 hasher = MD5.Create())
            {
                using (Stream stream = File.OpenRead(fileName))
                {
                    byte[] hash = hasher.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLower();
                }
            }
        }
    }
}
