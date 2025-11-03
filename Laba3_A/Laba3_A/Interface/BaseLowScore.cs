using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laba3_A.Interface
{
    class BaseLowScore : ILowScore
    {
        public float GetLowScore(NodeTree node)
        {
            var sumBakeOrder = node.BakedData.Select(x => node.Data.Z(x, node.BakedData.ToArray())).Sum();
            foreach (var oder in node.OpenData)
            {
                var bufBakeOrders = new List<int>(node.BakedData);
                bufBakeOrders.Add(oder);
                sumBakeOrder += node.Data.Z(oder, bufBakeOrders.ToArray());
            }
            return sumBakeOrder;
        }
    }
}

