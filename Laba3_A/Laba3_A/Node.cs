using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laba3_A
{
    public class NodeTree 
    {
        public List<NodeTree> NextNodes = new();
        public DataTask Data;
        public List<int> BakedData;
        public List<int> OpenData;
        public List<NodeTree> GetSheets()
        {
            var listNodes = new List<NodeTree>();
            if (NextNodes.Count == 0) 
            {
                listNodes.Add(this);
                return listNodes;
            }
            foreach (var node in NextNodes) 
            {
                listNodes.AddRange(node.GetSheets());
            }
            return listNodes;
        }
    }


}
