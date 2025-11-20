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

    // Улучшенный выбор ветвления:
    // 1) минимальный H (самый перспективный по нижней границе),
    // 2) при равенстве — минимальный (B - H) как оценка ширины зазора,
    // 3) затем минимальный B,
    // 4) затем меньшее количество открытых вершин (агрессивнее раскрывает узел).
    internal class OptimizedBranching : IBranching
    {
        public int Branch(IReadOnlyList<Leaf> leaves)
        {
            long bestH = long.MaxValue;
            long bestGap = long.MaxValue;
            long bestB = long.MaxValue;
            int bestOpen = int.MaxValue;
            int bestId = -1;
            
            for (int i = 0; i < leaves.Count; i++)
            {
                var leaf = leaves[i];
                if (leaf.OpenData.Count == 0) continue;

                long h = leaf.H;
                long b = leaf.B;
                long gap = b - h;
                int open = leaf.OpenData.Count;

                if (h < bestH ||
                    (h == bestH && gap < bestGap) ||
                    (h == bestH && gap == bestGap && b < bestB) ||
                    (h == bestH && gap == bestGap && b == bestB && open < bestOpen))
                {
                    bestH = h;
                    bestGap = gap;
                    bestB = b;
                    bestOpen = open;
                    bestId = i;
                }
            }
            
            return bestId;
        }
    }
}
