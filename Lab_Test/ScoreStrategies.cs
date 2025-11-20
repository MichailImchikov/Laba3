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

            long t = leaf.T;
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
                    foreach (var idx in remaining.OrderBy(x => x)) order.Add(idx);
                    leaf.OpenData.Clear();
                    leaf.OpenData.AddRange(order);
                    return failed + remaining.Count;
                }
                order.Add(minDiffId);
                t += times[lastVertId][minDiffId];
                remaining.Remove(minDiffId);
                lastVertId = minDiffId;
            }
            leaf.OpenData.Clear();
            leaf.OpenData.AddRange(order);
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
                if (leaf.T + times[last][a] > directiveTimes[a]) output++;
            }
            return output;
        }

    }

    // Оптимизированная версия использует более сильную верхнюю границу:
    // 1) Сначала пытается добавлять только «своевременные» вершины по минимальному резерву (как базовая)
    // 2) Если своевременных нет, выбирает вершину с минимальной просрочкой и продолжает построение маршрута,
    //    поскольку это может «разблокировать» другие вершины и уменьшить итоговое число опоздавших.
    // Это дает более плотную верхнюю границу (<= базовой), что улучшает отсечения.
    internal class OptimizedHighScore : IHighScore
    {
        public long ComputeB(Leaf leaf, Times times, DirectiveTimes directiveTimes)
        {
            var remaining = new HashSet<int>(leaf.OpenData);
            var order = new List<int>(remaining.Count);

            long t = leaf.T;
            int last = leaf.BakedData[^1];
            int failed = leaf.Failed;

            while (remaining.Count > 0)
            {
                // 1) Пытаемся найти своевременную вершину с минимальным резервом
                long bestSlack = long.MaxValue;
                int bestOnTime = -1;
                foreach (var v in remaining)
                {
                    long timeAfter = t + times[last][v];
                    if (timeAfter <= directiveTimes[v])
                    {
                        long slack = directiveTimes[v] - timeAfter;
                        if (slack < bestSlack)
                        {
                            bestSlack = slack;
                            bestOnTime = v;
                        }
                    }
                }

                int chosen;
                long arriveTime;
                bool isLate;

                if (bestOnTime != -1)
                {
                    chosen = bestOnTime;
                    arriveTime = t + times[last][chosen];
                    isLate = false;
                }
                else
                {
                    // 2) Своевременных кандидатов нет — выбираем с минимальной просрочкой
                    long minTardiness = long.MaxValue;
                    int bestLate = -1;
                    foreach (var v in remaining)
                    {
                        long timeAfter = t + times[last][v];
                        long tardiness = timeAfter - directiveTimes[v]; // > 0
                        if (tardiness < minTardiness)
                        {
                            minTardiness = tardiness;
                            bestLate = v;
                        }
                    }

                    chosen = bestLate;
                    arriveTime = t + times[last][chosen];
                    isLate = arriveTime > directiveTimes[chosen];
                }

                // Обновляем маршрут и метрики
                order.Add(chosen);
                if (isLate) failed++;
                t = arriveTime;
                last = chosen;
                remaining.Remove(chosen);
            }

            leaf.OpenData.Clear();
            leaf.OpenData.AddRange(order);
            return failed;
        }
    }

    // Логически усиленная нижняя граница:
    // 1) Считаем "немедленно поздние" вершины (как в BaseLowScore).
    // 2) Среди оставшихся ищем непересекающиеся пары (u,v), которые НЕ могут быть обе своевременными:
    //       T + time[last][u] + time[u][v] > D[v]  И
    //       T + time[last][v] + time[v][u] > D[u]
    //    Если оба направления дают опоздание второй вершины, то в любой последовательности
    //    хотя бы одна из (u,v) обязательно будет опоздала. Для набора непересекающихся пар
    //    это добавляет столько же обязательных опозданий.
    // Это усиливает нижнюю границу без риска завышения (корректно для отсечения).
    internal class OptimizedLowScore : ILowScore
    {
        public long ComputeH(Leaf leaf, Times times, DirectiveTimes directiveTimes)
        {
            long mandatoryLate = leaf.Failed;
            int last = leaf.BakedData[^1];
            long t0 = leaf.T;
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
}
