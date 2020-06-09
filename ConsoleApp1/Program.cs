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
            Queue<string> q = new Queue<string>();
            q.Enqueue("a");
            q.Enqueue("a");
            q.Enqueue("a");
            q.Enqueue("a");
            q.Enqueue("a");
            q.Enqueue("a");
            for (int i = 0; i < q.Count; i++)
            {
                Console.WriteLine($"{i} - {q.Dequeue()} - {q.Count}");
            }
        }
    }
}
