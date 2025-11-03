using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laba3_A
{
    class DataTask
    {
        public int CountOrders;
        public List<int> DirectiveTime = new();
        public int[,] TransitionMatrix;
        public DataTask(int countOrders, List<int> directiveTime, int[,] transitionMatrix)
        {
            CountOrders = countOrders;
            DirectiveTime = directiveTime;
            TransitionMatrix = transitionMatrix;
        }
    }
}
