using System.Collections.Generic;

namespace Lab_Test
{
    internal interface IBranching
    {
        // Selects the index of the leaf to branch next
        int Branch(IReadOnlyList<Leaf> leaves);
    }

    internal class BaseBranching : IBranching
    {
        // Default strategy: pick the leaf with minimal B among expandable leaves
        public int Branch(IReadOnlyList<Leaf> leaves)
        {
            long minB = long.MaxValue;
            int minBId = -1;
            for (int i = 0; i < leaves.Count; i++)
            {
                // Expandable if we still have open vertices to choose
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
            long min = long.MaxValue;
            int minBId = -1;
            for (int i = 0; i < leaves.Count; i++)
            {
                // Expandable if we still have open vertices to choose
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
