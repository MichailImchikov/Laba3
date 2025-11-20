using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab_Test
{
    using Times = List<List<long>>;
    using DirectiveTimes = List<long>;

    internal class Leaf
    {
        public List<int> OpenData { get; set; } = new();
        public List<int> BakedData { get; set; } = new();

        public long CurrentTime { get; set; }
        public int Failed { get; set; }
        public long B { get; set; }
        public long H { get; set; }

        public Leaf Copy()
        {
            return new Leaf
            {
                OpenData = new List<int>(OpenData),
                BakedData = new List<int>(BakedData),
                CurrentTime = this.CurrentTime,
                Failed = this.Failed,
                B = this.B,
                H = this.H
            };
        }
    }

    internal class BaseSolver
    {
        private readonly Times _times;
        private readonly DirectiveTimes _directiveTimes;
        private readonly int _n;
        private readonly IHighScore _highScore;
        private readonly ILowScore _lowScore;
        private readonly IBranching _branching;

        public BaseSolver(Times times, DirectiveTimes directiveTimes, IHighScore highScore, ILowScore lowScore, IBranching branching)
        {
            _times = times;
            _directiveTimes = directiveTimes;
            _highScore = highScore;
            _lowScore = lowScore;
            _branching = branching;
            _n = times.Count;
        }

        private Leaf UpdateLeaf(Leaf leaf, int a)
        {
            leaf.CurrentTime = leaf.CurrentTime + _times[leaf.BakedData[^1]][a];
            leaf.BakedData.Add(a);
            if (leaf.CurrentTime > _directiveTimes[a]) leaf.Failed++;
            if (leaf.OpenData.Count > 0)
            {
                var idx = leaf.OpenData.IndexOf(a);
                if (idx >= 0) leaf.OpenData.RemoveAt(idx);
            }
            leaf.B = _highScore.ComputeB(leaf, _times, _directiveTimes);
            leaf.H = _lowScore.ComputeH(leaf, _times, _directiveTimes);
            return leaf;
        }

        public Leaf BranchAndBound(out int leavesTraversed)
        {
            var root = new Leaf
            {
                BakedData = new List<int> { 0 },
                CurrentTime = 0,
                Failed = 0
            };
            root.OpenData = Enumerable.Range(0, _n).Where(i => i != 0).ToList();
            root.H = _lowScore.ComputeH(root, _times, _directiveTimes);
            root.B = _highScore.ComputeB(root, _times, _directiveTimes);
            var leaves = new List<Leaf> { root };
            leavesTraversed = 1;
            while (true)
            {
                int leafId = _branching.Branch(leaves);
                var leaf = leaves[leafId];
                var candidates = leaf.OpenData.ToList();
                foreach (var a in candidates)
                {
                    leaves.Add(UpdateLeaf(leaf.Copy(), a));
                    leavesTraversed++;
                }
                leaves.RemoveAt(leafId);

                int minBId = -1;
                long minB = long.MaxValue;
                for (int i = 0; i < leaves.Count; i++)
                {
                    if (leaves[i].B < minB)
                    {
                        minB = leaves[i].B;
                        minBId = i;
                    }
                }

                for (int i = 0; i < leaves.Count; i++)
                {
                    if (i == minBId) continue;
                    if (minB <= leaves[i].H)
                    {
                        leaves.RemoveAt(i);
                        if (i < minBId) minBId--;
                        i--;
                    }
                }
                if (leaves.Count == 1 && leaves[0].B == leaves[0].H)
                {
                    var leafFinal = leaves[0];
                    var output = leafFinal.Copy();
                    output.BakedData.AddRange(output.OpenData);
                    return output;
                }
            }
        }
    }
}
