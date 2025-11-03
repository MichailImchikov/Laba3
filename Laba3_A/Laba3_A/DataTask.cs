using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laba3_A
{
    public class DataTask
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
        public int Y(int i, int[] x)
        {
            for (int position = 0; position < x.Length; position++)
            {
                if (x[position] == i) return position;
            }
            return -1;
        }
        public int Z(int i, int[] x)
        {
            int yi = Y(i, x);
            int time = TransitionMatrix[0, x[0]];
            for (int index = 0; index < yi; index++)
            {
                time += TransitionMatrix[x[index], x[index + 1]];
            }
            return time;
        }
        public int W(int i, int[] x)
        {
            int deliveryTime = Z(i, x);
            return deliveryTime > DirectiveTime[i] ? 1 : 0;
        }
        public int CalculateCriterion(int[] currentOrder)
        {
            int sum = 0;
            for(int index=0; index < currentOrder.Length; index++)
            {
                sum += W(index, currentOrder);
            }
            return sum;
        }
    }
}
