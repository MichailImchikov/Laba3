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
            List<int> taskSize = new List<int>() { 3, 3, 10, 10, 10, 15, 15, 50, 50, 50 };
            List<(int, long)> baseAlg = new List<(int, long)>();
            List<(int, long)> customAlg = new List<(int, long)>();
            foreach (var task in tasks)
            {
                var baseSolver = new BaseSolver(task.Times, task.DirectiveTimes, new BaseHighScore(), new BaseLowScore(), new BaseBranching());
                var sw = Stopwatch.StartNew();
                var result = baseSolver.BranchAndBound(out var leavesTraversedBase);
                sw.Stop();
                Console.WriteLine($"____TASK{++i}____");

                var perm = result.BakedData.Where(v => v != 0);
                Console.WriteLine(string.Join(" ", perm));
                Console.WriteLine("Критерий: " + result.H);
                Console.WriteLine("Просмотрено вершин: " + leavesTraversedBase);
                Console.WriteLine("Время: " + sw.ElapsedMilliseconds + " ms");
                Console.WriteLine();
                baseAlg.Add((leavesTraversedBase, sw.ElapsedMilliseconds));
                var solver = new BaseSolver(task.Times, task.DirectiveTimes, new HighScore(), new LowScore(), new Branching());
                sw = Stopwatch.StartNew();
                result = solver.BranchAndBound(out var leavesTraversedCustom);
                sw.Stop();
                perm = result.BakedData.Where(v => v != 0);
                Console.WriteLine(string.Join(" ", perm));
                Console.WriteLine("Критерий: " + result.H);
                Console.WriteLine("Просмотрено вершин: " + leavesTraversedCustom);
                Console.WriteLine("Время: " + sw.ElapsedMilliseconds + " ms");
                Console.WriteLine();
                customAlg.Add((leavesTraversedCustom, sw.ElapsedMilliseconds));
            }
            Console.WriteLine(" N  " + "Базовый  " + "Собственный " + "Оценка времени " + "Оценка вершин");
            double fractionBase = 0;
            double fractionCustom = 0;
            List<float> diffTime = new List<float>();
            List<float>diffLeaf = new List<float>();
            for(int index = 0; index < taskSize.Count; index++)
            {
                if (baseAlg[index].Item2 == 0)
                {
                    diffTime.Add(baseAlg[index].Item2 - customAlg[index].Item2);
                }
                else diffTime.Add((float)(baseAlg[index].Item2 - customAlg[index].Item2) / baseAlg[index].Item2);
                diffLeaf.Add((float)(baseAlg[index].Item1 - customAlg[index].Item1)/ baseAlg[index].Item1);
                Console.WriteLine($"{index + 1,2} {baseAlg[index].Item1,8}     {customAlg[index].Item1,8} {diffTime[index]:0.0000}         {diffLeaf[index]:0.0000}");
            }
            float percentTime = 0;
            float percentLeaf = 0;
            for(int index = 0;index < taskSize.Count; index++)
            {
                percentTime += diffTime[index];
                percentLeaf += diffLeaf[index];
            }
            percentTime/=taskSize.Count;
            percentLeaf/=taskSize.Count;
            Console.WriteLine($"{percentTime * 100}   {percentLeaf * 100}");
        }
        static public long Factorial(int n)
        {
            if (n == 1) return 1;

            return n * Factorial(n - 1);
        }
    }
}
