using Laba3_A.Data;

namespace Laba3_A
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var dates = DataTaskReader.LoadAll();
            var Low = new BaseLowScore();
            var High = new BaseHighScore();
            var Branchong = new BaseBranching(High);
            var baseBransfin = new BranchBoundMethod(Branchong, Low, High);
            Console.WriteLine(baseBransfin.GetDecisionn(dates[0]).Perest);
        }
    }
}
