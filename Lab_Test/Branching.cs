using System.Collections.Generic;

namespace Lab_Test
{
    internal interface IBranching
    {
        int Branch(IReadOnlyList<Leaf> leaves);
    }

    internal class BaseBranching : IBranching
    {
        public int Branch(IReadOnlyList<Leaf> leaves)
        {
            long minB = long.MaxValue;
            int minBId = -1;
            for (int i = 0; i < leaves.Count; i++)
            {
                if (leaves[i].OpenData.Count > 0 && leaves[i].B < minB)
                {
                    minB = leaves[i].B;
                    minBId = i;
                }
            }
            return minBId;
        }
    }
}
