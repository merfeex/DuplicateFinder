using DuplicateFinder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ConsoleApp1
{
    class Program
    {
        static void Main()
        {
            Console.Write("Path: ");
            string[] path = Console.ReadLine().Split(" ");
            var dict = Finder.FindDuplicates(path);
            foreach(var d in dict)
            {
                if (d.Value.Count > 1)
                {
                    Console.Write(d.Key + $"!{d.Value.Count}!: ");
                    foreach (string s in d.Value)
                    {
                        Console.Write($"[{s}] ");
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}
