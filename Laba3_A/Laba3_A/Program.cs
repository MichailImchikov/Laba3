using Laba3_A.Data;
using System.Diagnostics; // добавлено для измерения времени

namespace Laba3_A
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var dates = DataTaskReader.LoadAll();
            var Low = new BaseLowScore();
            var High = new BaseHighScore();
            var Branchong = new BaseBranching();
            var baseBransfin = new BranchBoundMethod(Branchong, Low, High);
            int i = 0;
            foreach (var task in dates)
            {
                Console.WriteLine("____TASK" + ++i + "____");
                var sw = Stopwatch.StartNew(); // старт измерения времени
                var res = baseBransfin.GetDecisionn(task);
                sw.Stop(); // остановка
                Console.WriteLine("Перестановка: " + string.Join(" ", res.Perest));
                Console.WriteLine("Критерий: " + task.CalculateCriterion(res.Perest.ToArray()));
                Console.WriteLine($"Количество просмотренных листьев: {res.CountLeaf}");
                Console.WriteLine($"Время выполнения задачи: {sw.ElapsedMilliseconds} ms");
                Console.WriteLine();
            }

        }
    }
}
