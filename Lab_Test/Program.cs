using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lab_Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var tasks = DataTaskLoader.LoadAll();
            if (tasks.Count == 0) return;

            foreach (var task in tasks)
            {
                var baseSolver = new BaseSolver(task.Times, task.DirectiveTimes, new BaseHighScore(), new BaseLowScore(), new BaseBranching());
                var result = baseSolver.BranchAndBound(out _);
                //var solver = new BaseSolver(task.Times, task.DirectiveTimes, new HighScore(), new LowScore(), new Branching());

                var perm = result.BakedData.Where(v => v != 0);
                Console.WriteLine(string.Join(" ", perm) + " " + result.H);
            }
        }
    }
}
