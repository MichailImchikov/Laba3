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
            long minB = leaves[0].OpenData.Count + leaves[0].BakedData.Count;
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
    internal class Branching : IBranching
    {
        public int Branch(IReadOnlyList<Leaf> leaves)
        {
            long min = leaves[0].OpenData.Count + leaves[0].BakedData.Count;
            int minBId = -1;

            for (int i = 0; i < leaves.Count; i++)
            {
                if (leaves[i].OpenData.Count > 0 && leaves[i].B - leaves[i].H < min)
                {
                    min = leaves[i].B - leaves[i].H;
                    minBId = i;
                }
            }
            return minBId;
        }
    }
}
