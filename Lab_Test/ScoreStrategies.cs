
namespace Lab_Test
{
    using Times = List<List<long>>;
    using DirectiveTimes = List<long>;

    public interface IHighScore
    {
        long GetScoreB(Node node, Times times, DirectiveTimes directiveTimes);
    }

    public interface ILowScore
    {
        long GetScoreH(Node node, Times times, DirectiveTimes directiveTimes);
    }
    public interface IBranching
    {
        int Branch(IReadOnlyList<Node> leaves);
    }

    public class BaseHighScore : IHighScore
    {
        public virtual long GetScoreB(Node node, Times times, DirectiveTimes directiveTimes)
        {
            var remaining = new HashSet<int>(node.FreeOrder);
            var order = new List<int>(remaining.Count);

            long t = node.T;
            int lastVertId = node.CloseOrder[^1];
            int failed = node.Failed;
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
                    node.FreeOrder.Clear();
                    node.FreeOrder.AddRange(order);
                    return failed + remaining.Count;
                }
                order.Add(minDiffId);
                t += times[lastVertId][minDiffId];
                remaining.Remove(minDiffId);
                lastVertId = minDiffId;
            }
            node.FreeOrder.Clear();
            node.FreeOrder.AddRange(order);
            return failed;
        }
    }

    public class BaseLowScore : ILowScore
    {
        public virtual long GetScoreH(Node node, Times times, DirectiveTimes directiveTimes)
        {
            long output = node.Failed;
            int last = node.CloseOrder[^1];
            foreach (var a in node.FreeOrder)
            {
                if (node.T + times[last][a] > directiveTimes[a]) output++;
            }
            return output;
        }

    }
    public class OptimizedHighScore : IHighScore
    {
        public virtual long GetScoreB(Node node, Times times, DirectiveTimes directiveTimes)
        {
            var remaining = new List<int>(node.FreeOrder);
            long t = node.T;
            int last = node.CloseOrder[^1];
            int failed = node.Failed;

            while (remaining.Count > 0)
            {
                int bestId = -1;
                int maxFeasibleAfter = -1;  
                long minSlack = long.MaxValue;
                long minDist = long.MaxValue;

                foreach (int i in remaining)
                {
                    long arrival = t + times[last][i];
                    if (arrival > directiveTimes[i]) continue; 

                    long newTime = arrival;
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
                if (bestId == -1)
                {
                    long minLateness = long.MaxValue;
                    long minDistForLate = long.MaxValue;

                    foreach (int i in remaining)
                    {
                        long arrival = t + times[last][i];
                        long lateness = arrival - directiveTimes[i]; 

                        bool isBetter = lateness < minLateness ||
                                       (lateness == minLateness && times[last][i] < minDistForLate);

                        if (isBetter)
                        {
                            minLateness = lateness;
                            minDistForLate = times[last][i];
                            bestId = i;
                        }
                    }
                    failed++; 
                }

                t += times[last][bestId];
                remaining.Remove(bestId);
                last = bestId;
            }

            return failed;
        }
    }

    public class OptimizedLowScore : ILowScore
    {
        public long GetScoreH(Node node, Times times, DirectiveTimes directiveTimes)
        {
            long mandatoryLate = node.Failed;
            int last = node.CloseOrder[^1];
            long t0 = node.T;
            var open = node.FreeOrder;

            if (open.Count == 0)
                return mandatoryLate;

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
                    candidates.Add(v); 
                }
            }

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
                        conflictPairs++;
                        used.Add(u);
                        used.Add(v);
                        break; 
                    }
                }
            }

            return mandatoryLate + conflictPairs;
        }
    }
    public class BaseBranching : IBranching
    {
        public int Branch(IReadOnlyList<Node> leaves)
        {
            long minB = long.MaxValue;
            int minBId = -1;
            for (int i = 0; i < leaves.Count; i++)
            {
                if (leaves[i].FreeOrder.Count > 0 && leaves[i].B < minB)
                {
                    minB = leaves[i].B;
                    minBId = i;
                }
            }
            return minBId;
        }
    }
    public class OptimizedBranching : IBranching
    {
        public int Branch(IReadOnlyList<Node> leaves)
        {
            long bestH = long.MaxValue;
            long bestGap = long.MaxValue;
            long bestB = long.MaxValue;
            int bestOpen = int.MaxValue;
            int bestId = -1;

            for (int i = 0; i < leaves.Count; i++)
            {
                var leaf = leaves[i];
                if (leaf.FreeOrder.Count == 0) continue;

                long h = leaf.H;
                long b = leaf.B;
                long gap = b - h;
                int open = leaf.FreeOrder.Count;

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
