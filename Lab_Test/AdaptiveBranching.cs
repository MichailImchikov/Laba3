using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab_Test
{
    // Адаптивная стратегия ветвления под текущие структуры Leaf / BaseSolver
    // Критерии выбора узла:
    // 1. Минимальный верхний предел (Leaf.B)
    // 2. При равенстве: больше безопасных вершин (не добавят штраф, если выбрать следующими)
    // 3. При равенстве: больший фактор ветвления (кол-во доступных вершин)
    // 4. При равенстве: меньший разрыв между верхним и нижним пределом (B - H)
    internal sealed class AdaptiveBranching : IBranching
    {
        private readonly List<List<long>> _times;
        private readonly List<long> _directiveTimes;
        public AdaptiveBranching(List<List<long>> times, List<long> directiveTimes)
        {
            _times = times ?? throw new ArgumentNullException(nameof(times));
            _directiveTimes = directiveTimes ?? throw new ArgumentNullException(nameof(directiveTimes));
        }

        public int Branch(IReadOnlyList<Leaf> leaves)
        {
            if (leaves == null || leaves.Count == 0) throw new ArgumentException("leaves must be non-empty", nameof(leaves));
            // Кандидаты: узлы у которых есть что расширять
            var candidateIndices = new List<int>();
            for (int i = 0; i < leaves.Count; i++)
            {
                if (leaves[i].OpenData.Count > 0) candidateIndices.Add(i);
            }
            if (candidateIndices.Count == 0) return 0; // fallback

            int bestIdx = candidateIndices[0];
            long bestUpper = long.MaxValue;
            int bestSafe = -1;
            int bestBranchFactor = -1;
            long bestGap = long.MaxValue;

            foreach (var idx in candidateIndices)
            {
                var node = leaves[idx];
                long upper = node.B; // верхний предел (оценка полной ошибки)
                long lower = node.H; // нижний предел (оптимистичная оценка)
                long gap = upper - lower;

                int lastVert = node.BakedData[^1];
                int safeCount = 0;
                foreach (var v in node.OpenData)
                {
                    long timeAfter = node.T + _times[lastVert][v];
                    if (timeAfter <= _directiveTimes[v]) safeCount++; // не добавит штраф
                }
                int branchFactor = node.OpenData.Count;

                bool better = false;
                if (upper < bestUpper) better = true;
                else if (upper == bestUpper && safeCount > bestSafe) better = true;
                else if (upper == bestUpper && safeCount == bestSafe && branchFactor > bestBranchFactor) better = true;
                else if (upper == bestUpper && safeCount == bestSafe && branchFactor == bestBranchFactor && gap < bestGap) better = true;

                if (better)
                {
                    bestIdx = idx;
                    bestUpper = upper;
                    bestSafe = safeCount;
                    bestBranchFactor = branchFactor;
                    bestGap = gap;
                }
            }
            return bestIdx;
        }
    }
}
