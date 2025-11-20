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

    // Оптимизированная стратегия: минимальное H (best-first search по нижней границе)
    // Выбирает самый перспективный лист с точки зрения оптимистичной оценки
    // При равном H предпочитает лист с минимальным B для стабильности
    internal class OptimizedBranching : IBranching
    {
        public int Branch(IReadOnlyList<Leaf> leaves)
        {
            long minH = long.MaxValue;
            long minB = long.MaxValue;
            int bestId = -1;
            
            for (int i = 0; i < leaves.Count; i++)
            {
                if (leaves[i].OpenData.Count > 0)
                {
                    long h = leaves[i].H;
                    long b = leaves[i].B;
                    
                    // Выбираем лист с минимальным H (лучший случай)
                    // При равном H предпочитаем лист с минимальным B
                    if (h < minH || (h == minH && b < minB))
                    {
                        minH = h;
                        minB = b;
                        bestId = i;
                    }
                }
            }
            
            return bestId;
        }
    }
}
