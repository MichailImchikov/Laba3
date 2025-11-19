using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace Lab_Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var tasks = DataTaskLoader.LoadAll();
            if (tasks.Count == 0) return;
            int i = 0;
            foreach (var task in tasks)
            {
                var branching = /*new AdaptiveBranching(task.Times, task.DirectiveTimes);*/ new Branching()/* new BaseBranching()*/;
                var baseSolver = new BaseSolver(task.Times, task.DirectiveTimes, new BaseHighScore(), new BaseLowScore(), new BaseBranching());
                var sw = Stopwatch.StartNew();
                var result = baseSolver.BranchAndBound(out var leavesTraversed);
                sw.Stop();
                Console.WriteLine($"____TASK{++i}____");

                var perm = result.BakedData.Where(v => v != 0);
                Console.WriteLine(string.Join(" ", perm));
                Console.WriteLine("Критерий: " + result.H);
                int depth = Math.Max(0, result.BakedData.Count - 1);
                Console.WriteLine("Глубина: " + depth);
                Console.WriteLine("Просмотрено вершин: " + leavesTraversed);
                Console.WriteLine("Время: " + sw.ElapsedMilliseconds + " ms");
                Console.WriteLine();
                var solver = new BaseSolver(task.Times, task.DirectiveTimes, new HighScore(), new LowScore(), new Branching());
                sw = Stopwatch.StartNew();
                result = solver.BranchAndBound(out var leavesTraversed1);
                sw.Stop();
                perm = result.BakedData.Where(v => v != 0);
                Console.WriteLine(string.Join(" ", perm));
                Console.WriteLine("Критерий: " + result.H);
                depth = Math.Max(0, result.BakedData.Count - 1);
                Console.WriteLine("Глубина: " + depth);
                Console.WriteLine("Просмотрено вершин: " + leavesTraversed1);
                Console.WriteLine("Время: " + sw.ElapsedMilliseconds + " ms");
                Console.WriteLine();
            }
        }
    }
}
