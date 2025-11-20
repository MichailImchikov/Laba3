using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab_Test
{
    using Times = List<List<long>>;
    using DirectiveTimes = List<long>;

    internal interface IHighScore
    {
        long ComputeB(Leaf leaf, Times times, DirectiveTimes directiveTimes);
    }

    internal interface ILowScore
    {
        long ComputeH(Leaf leaf, Times times, DirectiveTimes directiveTimes);
    }

    internal class BaseHighScore : IHighScore
    {
        public virtual long ComputeB(Leaf leaf, Times times, DirectiveTimes directiveTimes)
        {
            var remaining = new HashSet<int>(leaf.OpenData);
            var order = new List<int>(remaining.Count);

            long t = leaf.CurrentTime;
            int lastVertId = leaf.BakedData[^1];
            int failed = leaf.Failed;
            while (remaining.Count > 0)
            {
                long minDiff = long.MaxValue;
                int minDiffId = -1;
                foreach (var index in remaining)
                {
                    long timeAfter = t + times[lastVertId][index];
                    if (timeAfter > directiveTimes[index]) continue;
                    long diff = directiveTimes[index] - timeAfter;
                    if (diff < minDiff)
                    {
                        minDiff = diff;
                        minDiffId = index;
                    }
                }
                if (minDiffId == -1)
                {
                    //foreach (var idx in remaining.OrderBy(x => x)) order.Add(idx);
                    //leaf.OpenData.Clear();
                    //leaf.OpenData.AddRange(order);
                    return failed + remaining.Count;
                }
                order.Add(minDiffId);
                t += times[lastVertId][minDiffId];
                remaining.Remove(minDiffId);
                lastVertId = minDiffId;
            }
            //leaf.OpenData.Clear();
            //leaf.OpenData.AddRange(order);
            return failed;
        }
    }

    internal class BaseLowScore : ILowScore
    {
        public virtual long ComputeH(Leaf leaf, Times times, DirectiveTimes directiveTimes)
        {
            long output = leaf.Failed;
            int last = leaf.BakedData[^1];
            foreach (var a in leaf.OpenData)
            {
                if (leaf.CurrentTime + times[last][a] > directiveTimes[a]) output++;
            }
            return output;
        }
        
    }
    internal class AnotherLowScore : ILowScore
    {
        public long ComputeH(Leaf leaf, Times times, DirectiveTimes directiveTimes)
        {
            long lower = leaf.Failed;
            int last = leaf.BakedData[^1];
            long t0 = leaf.CurrentTime;

            var open = leaf.OpenData;
            if (open.Count == 0)
                return lower;

            // Собираем список ещё потенциальных (пока не доказано, что опоздают).
            var candidates = new List<int>(open.Count);

            // 1) Двухшаговая оптимистическая достижимость
            foreach (var v in open)
            {
                long direct = times[last][v];

                long viaAny = long.MaxValue;
                // Один промежуточный узел k
                foreach (var k in open)
                {
                    if (k == v) continue;
                    long path = times[last][k] + times[k][v];
                    if (path < viaAny) viaAny = path;
                }

                long bestArrival = t0 + Math.Min(direct, viaAny);

                if (bestArrival > directiveTimes[v])
                {
                    // Уже не может успеть даже при сверх-оптимизме
                    lower++;
                }
                else
                {
                    candidates.Add(v);
                }
            }

            if (candidates.Count <= 1)
                return lower;

            // 2) Упаковка по дедлайнам (дополнительные обязательные опоздания)
            // Глобально минимальное ребро между любыми двумя открытыми вершинами для сверхоптимизма.
            long globalMinEdge = long.MaxValue;
            for (int i = 0; i < open.Count; i++)
            {
                int a = open[i];
                for (int j = 0; j < open.Count; j++)
                {
                    if (i == j) continue;
                    long w = times[a][open[j]];
                    if (w < globalMinEdge) globalMinEdge = w;
                }
            }
            if (globalMinEdge == long.MaxValue)
                globalMinEdge = 0; // защита (если одна вершина)

            // Сортировка оставшихся по дедлайнам
            candidates.Sort((a, b) => directiveTimes[a].CompareTo(directiveTimes[b]));

            long optimisticTime = t0;
            int packingMandatory = 0;

            // Первую вершину считаем прибытие через минимально возможный прямой/двухшаговый путь (уже учтено выше).
            // Для упрощения: стартуем как будто "прыжок" к первой вершине не стоит времени (ещё более оптимистично).
            // Это не ухудшает корректность нижней границы.
            for (int idx = 0; idx < candidates.Count; idx++)
            {
                int v = candidates[idx];

                // Прибавляем минимально возможное время перехода между абстрактными соседними посещениями.
                if (idx == 0)
                {
                    // Нулевой расход времени — сверх оптимизм
                }
                else
                {
                    optimisticTime += globalMinEdge;
                }

                if (optimisticTime > directiveTimes[v])
                {
                    packingMandatory++;
                }
            }

            return lower + packingMandatory;
        }
    }
    internal class LowScore : ILowScore
    {
        public virtual long ComputeH(Leaf leaf, Times times, DirectiveTimes directiveTimes)
        {
            long mandatoryLate = leaf.Failed;
            int last = leaf.BakedData[^1];
            long t0 = leaf.CurrentTime;
            var open = leaf.OpenData;

            if (open.Count == 0)
                return mandatoryLate;

            // 1) Немедленно поздние
            var candidates = new List<int>(open.Count);
            foreach (var v in open)
            {
                long arrival = t0 + times[last][v];
                if (arrival > directiveTimes[v])
                {
                    mandatoryLate++;
                }
                else
                {
                    candidates.Add(v); // потенциально своевременные
                }
            }

            // 2) Поиск непересекающихся конфликтных пар
            //    Жадный подбор пар (matching): каждая добавляет +1 к гарантированным опозданиям.
            int m = candidates.Count;
            if (m <= 1)
                return mandatoryLate;

            var used = new HashSet<int>();
            int conflictPairs = 0;

            for (int i = 0; i < m; i++)
            {
                int u = candidates[i];
                if (used.Contains(u)) continue;

                for (int j = i + 1; j < m; j++)
                {
                    int v = candidates[j];
                    if (used.Contains(v)) continue;

                    long arriveUFirst = t0 + times[last][u];
                    long arriveUThenV = arriveUFirst + times[u][v];

                    long arriveVFirst = t0 + times[last][v];
                    long arriveVThenU = arriveVFirst + times[v][u];

                    bool bothCannotBeOnTime =
                        arriveUThenV > directiveTimes[v] &&
                        arriveVThenU > directiveTimes[u];

                    if (bothCannotBeOnTime)
                    {
                        // Добавляем одну обязательную просрочку (для пары)
                        conflictPairs++;
                        // Помечаем обе, чтобы не образовывать пересечений (matching)
                        used.Add(u);
                        used.Add(v);
                        break; // переходим к следующему i
                    }
                }
            }

            return mandatoryLate + conflictPairs;
        }
    }
    internal class HighScore : IHighScore
    {
        public virtual long ComputeB(Leaf leaf, Times times, DirectiveTimes directiveTimes)
        {
            var remaining = new HashSet<int>(leaf.OpenData);
            var order = new List<int>(remaining.Count);

            long t = leaf.CurrentTime;
            int lastVertId = leaf.BakedData[^1];
            int failed = leaf.Failed;
            while (remaining.Count > 0)
            {
                long minDiff = long.MaxValue;
                int minDiffId = -1;
                foreach (var index in remaining)
                {
                    long timeAfter = t + times[lastVertId][index];
                    if (timeAfter > directiveTimes[index]) continue;
                    //long diff = directiveTimes[index] - timeAfter;
                    if (timeAfter < minDiff)
                    {
                        minDiff = timeAfter;
                        minDiffId = index;
                    }
                }
                if (minDiffId == -1)
                {
                    // No feasible next; the rest will be late. Append them in ascending order for determinism.
                    //foreach (var idx in remaining.OrderBy(x => x)) order.Add(idx);
                    // persist the computed order into OpenData
                    //leaf.OpenData.Clear();
                    //leaf.OpenData.AddRange(order);
                    return failed + remaining.Count;
                }
                order.Add(minDiffId);
                t += times[lastVertId][minDiffId];
                remaining.Remove(minDiffId);
                lastVertId = minDiffId;
            }
            // persist the computed order into OpenData
            //leaf.OpenData.Clear();
            //leaf.OpenData.AddRange(order);
            return failed;
        }
    }
}
