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
                    return failed + remaining.Count;
                }
                order.Add(minDiffId);
                t += times[lastVertId][minDiffId];
                remaining.Remove(minDiffId);
                lastVertId = minDiffId;
            }
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
            var remaining = new List<int>(leaf.OpenData);
            long t = leaf.CurrentTime;
            int last = leaf.BakedData[^1];
            int failed = leaf.Failed;

            while (remaining.Count > 0)
            {
                int bestId = -1;
                int maxFeasibleAfter = -1;  // Сколько успевающих останется после выбора
                long minSlack = long.MaxValue;
                long minDist = long.MaxValue;

                // === Шаг 1: перебираем всех, кто УСПЕВАЕТ СЕЙЧАС ===
                foreach (int i in remaining)
                {



                    long arrival = t + times[last][i];
                    if (arrival > directiveTimes[i]) continue; // пропускаем нарушителей

                    // Lookahead-1: сколько заказов будут успевать ПОСЛЕ выполнения i?
                    long newTime = arrival;
                    //int feasibleAfter = 0;


                    int feasibleAfter = 0;
                    int checkedCount = 0;
                    int maxCheck = Math.Min(remaining.Count - 1, 5);

                    foreach (int j in remaining)
                    {
                        if (j == i) continue;
                        if (++checkedCount > maxCheck) break;
                        if (newTime + times[i][j] <= directiveTimes[j])
                            feasibleAfter++;
                    }

                    // Критерий выбора:
                    // 1. Максимизируем feasibleAfter (главный приоритет)
                    // 2. При равенстве — минимизируем slack (наиболее срочный)
                    // 3. При равенстве — минимизируем расстояние (ближе)
                    long slack = directiveTimes[i] - arrival;

                    bool isBetter = feasibleAfter > maxFeasibleAfter ||
                                   (feasibleAfter == maxFeasibleAfter && slack < minSlack) ||
                                   (feasibleAfter == maxFeasibleAfter && slack == minSlack && times[last][i] < minDist);

                    if (isBetter)
                    {
                        maxFeasibleAfter = feasibleAfter;
                        minSlack = slack;
                        minDist = times[last][i];
                        bestId = i;
                    }
                }

                // === Шаг 2: если никто не успевает СЕЙЧАС — выбираем наименее нарушающего ===
                if (bestId == -1)
                {
                    long minLateness = long.MaxValue;
                    long minDistForLate = long.MaxValue;

                    foreach (int i in remaining)
                    {
                        long arrival = t + times[last][i];
                        long lateness = arrival - directiveTimes[i]; // > 0

                        bool isBetter = lateness < minLateness ||
                                       (lateness == minLateness && times[last][i] < minDistForLate);

                        if (isBetter)
                        {
                            minLateness = lateness;
                            minDistForLate = times[last][i];
                            bestId = i;
                        }
                    }
                    failed++; // нарушение
                }

                // === Применяем выбор ===
                t += times[last][bestId];
                remaining.Remove(bestId);
                last = bestId;
            }

            return failed;
        }
    }
}
