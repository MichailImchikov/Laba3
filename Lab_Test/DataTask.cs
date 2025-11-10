using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_Test
{
    public class DataTask
    {
        public List<List<long>> Times { get; }
        public List<long> DirectiveTimes { get; }
        public string SourcePath { get; }
        public DataTask(List<List<long>> times, List<long> directiveTimes, string sourcePath)
        {
            Times = times;
            DirectiveTimes = directiveTimes;
            SourcePath = sourcePath;
        }
    }
}
