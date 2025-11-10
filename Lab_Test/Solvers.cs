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

        public long T { get; set; }
        public int Failed { get; set; }
        public long B { get; set; }
        public long H { get; set; }

        // ¬озвращает глубокую копию текущего листа
        public Leaf Copy()
        {
            return new Leaf
            {
                OpenData = new List<int>(OpenData),
                BakedData = new List<int>(BakedData),
                T = this.T,
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
        private readonly int _n; // n+1 of original logic
        private readonly IHighScore _highScore;
        private readonly ILowScore _lowScore;
        private readonly IBranching _branching;

        public BaseSolver(Times times, DirectiveTimes directiveTimes, IHighScore highScore, ILowScore lowScore, IBranching branching)
        {
            _times = times ?? throw new ArgumentNullException(nameof(times));
            _directiveTimes = directiveTimes ?? throw new ArgumentNullException(nameof(directiveTimes));
            _highScore = highScore ?? throw new ArgumentNullException(nameof(highScore));
            _lowScore = lowScore ?? throw new ArgumentNullException(nameof(lowScore));
            _branching = branching ?? throw new ArgumentNullException(nameof(branching));
            _n = times.Count; // contains start vertex
            if (_directiveTimes.Count != _n) throw new ArgumentException("directiveTimes size mismatch times");
            if (_times.Count == 0 || _times[0].Count != _n) throw new ArgumentException("times row size mismatch n");
            if (_directiveTimes[0] != 0) throw new ArgumentException("directive_times[0] must be 0");
        }

        private Leaf UpdateLeaf(Leaf leaf, int a)
        {
            leaf.T = leaf.T + _times[leaf.BakedData[^1]][a];
            leaf.BakedData.Add(a);
            if (leaf.T > _directiveTimes[a]) leaf.Failed++;
            // keep OpenData in sync
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
                T = 0,
                Failed = 0
            };
            // initialize OpenData as indices except 0
            root.OpenData = Enumerable.Range(0, _n).Where(i => i != 0).ToList();
            root.H = _lowScore.ComputeH(root, _times, _directiveTimes);
            root.B = _highScore.ComputeB(root, _times, _directiveTimes);
            var leaves = new List<Leaf> { root };
            leavesTraversed = 1;
            while (true)
            {
                int leafId = _branching.Branch(leaves);
                var leaf = leaves[leafId];
                // branch only over currently open vertices
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
