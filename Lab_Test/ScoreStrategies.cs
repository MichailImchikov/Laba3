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
}
