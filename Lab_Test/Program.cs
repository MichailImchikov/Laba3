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
            
            Console.WriteLine("=== СРАВНЕНИЕ БАЗОВОГО И ОПТИМИЗИРОВАННОГО АЛГОРИТМОВ ===\n");
            
            int taskNumber = 0;
            int totalBaseLeaves = 0;
            int totalOptimizedLeaves = 0;
            int improvedTasks = 0;
            
            foreach (var task in tasks)
            {
                taskNumber++;
                Console.WriteLine($"====== ЗАДАЧА {taskNumber} ======");

                // БАЗОВЫЙ АЛГОРИТМ
                var baseBranching = new BaseBranching();
                var baseSolver = new BaseSolver(
                    task.Times,
                    task.DirectiveTimes,
                    new BaseHighScore(),
                    new BaseLowScore(),
                    baseBranching
                );
                
                var sw1 = Stopwatch.StartNew();
                var baseResult = baseSolver.BranchAndBound(out var baseLeaves);
                sw1.Stop();

                // ОПТИМИЗИРОВАННЫЙ АЛГОРИТМ
                var optimizedBranching = new OptimizedBranching();
                var optimizedSolver = new BaseSolver(
                    task.Times,
                    task.DirectiveTimes,
                    new OptimizedHighScore(),
                    new OptimizedLowScore(),
                    optimizedBranching
                );
                
                var sw2 = Stopwatch.StartNew();
                var optimizedResult = optimizedSolver.BranchAndBound(out var optimizedLeaves);
                sw2.Stop();

                // Вывод результатов
                Console.WriteLine("\nБАЗОВЫЙ:");
                var basePerm = baseResult.BakedData.Where(v => v != 0);
                Console.WriteLine($"  Маршрут: {string.Join(" ", basePerm)}");
                Console.WriteLine($"  Критерий: {baseResult.H}");
                Console.WriteLine($"  Просмотрено вершин: {baseLeaves}");
                Console.WriteLine($"  Время: {sw1.ElapsedMilliseconds} мс");

                Console.WriteLine("\nОПТИМИЗИРОВАННЫЙ:");
                var optPerm = optimizedResult.BakedData.Where(v => v != 0);
                Console.WriteLine($"  Маршрут: {string.Join(" ", optPerm)}");
                Console.WriteLine($"  Критерий: {optimizedResult.H}");
                Console.WriteLine($"  Просмотрено вершин: {optimizedLeaves}");
                Console.WriteLine($"  Время: {sw2.ElapsedMilliseconds} мс");

                // Сравнение
                Console.WriteLine("\nСРАВНЕНИЕ:");
                
                bool criterionMatch = baseResult.H == optimizedResult.H;
                if (criterionMatch)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  ✓ Критерий совпадает: {baseResult.H}");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  ✗ ОШИБКА! Критерии не совпадают: {baseResult.H} vs {optimizedResult.H}");
                    Console.ResetColor();
                }

                int diff = baseLeaves - optimizedLeaves;
                double improvement = baseLeaves > 0 ? ((double)diff / baseLeaves * 100) : 0;
                
                if (diff > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  ✓ УЛУЧШЕНИЕ: на {diff} вершин меньше ({improvement:F2}%)");
                    Console.ResetColor();
                    if (criterionMatch) improvedTasks++;
                }
                else if (diff < 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"  ⚠ на {-diff} вершин больше ({-improvement:F2}%)");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"  = Одинаковое количество вершин");
                }

                totalBaseLeaves += baseLeaves;
                totalOptimizedLeaves += optimizedLeaves;
                
                Console.WriteLine();
            }

            // ИТОГОВАЯ СТАТИСТИКА
            Console.WriteLine("==========================================");
            Console.WriteLine("ИТОГОВАЯ СТАТИСТИКА:");
            Console.WriteLine($"Всего задач: {taskNumber}");
            Console.WriteLine($"Задач с улучшением: {improvedTasks}/{taskNumber}");
            Console.WriteLine($"Всего вершин базовый: {totalBaseLeaves:N0}");
            Console.WriteLine($"Всего вершин оптимизированный: {totalOptimizedLeaves:N0}");
            
            int totalDiff = totalBaseLeaves - totalOptimizedLeaves;
            double totalImprovement = totalBaseLeaves > 0 ? ((double)totalDiff / totalBaseLeaves * 100) : 0;
            
            if (totalDiff > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Общее улучшение: {totalDiff:N0} вершин ({totalImprovement:F2}%)");
                Console.ResetColor();
            }
            else if (totalDiff < 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Общее ухудшение: {-totalDiff:N0} вершин ({-totalImprovement:F2}%)");
                Console.ResetColor();
            }
            Console.WriteLine("==========================================");
            
            Console.WriteLine("\nНажмите любую клавишу для завершения...");
            Console.ReadKey();
        }
    }
}
