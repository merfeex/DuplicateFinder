using DuplicateFinder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main()
        {
            string path = @"C:\Users\merfeex";
            var dups = Finder.FindDuplicates(path);
            foreach (var kvp in dups)
            {
                Console.WriteLine(kvp.Key);
                foreach (var v in kvp.Value)
                {
                    Console.WriteLine(v);
                }
                Console.WriteLine(new string('-', 80));
            }
        }
    }
}
