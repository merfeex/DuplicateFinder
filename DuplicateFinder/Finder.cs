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
            Task[] tasks = new Task[paths.Length];
            var dict = new ConcurrentDictionary<string, IList<string>>();
            int cnt = 0;
            foreach(string path in paths)
            {
                if (!File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                {
                    throw new ArgumentException("Not a directory: " + path);
                }
                tasks[cnt] = Task.Run(() => HandleFiles(path, dict));
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

        private static void HandleFiles(string path, ConcurrentDictionary<string, IList<string>> dict)
        {
            // exception on App Data folder
            string[] directories = Directory.GetDirectories(path);
            string[] files = Directory.GetFiles(path);
            Task[] tasks = new Task[directories.Length];
            int cnt = 0;
            foreach (string s in directories)
            {
                try
                {
                    tasks[cnt] = Task.Run(() => HandleFiles(s, dict));
                    cnt++;
                }
                catch (IOException) { }
            }
            for (int i = 0; i < files.Length; i++)
            {
                try
                {
                    string hash = GetHash(files[i]);
                    dict.AddOrUpdate(hash, new List<string> { files[i] }, (k, v) => { v.Add(files[i]); return v; });
                }
                catch (IOException) { }
            }
            Task.WaitAll(tasks);
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
