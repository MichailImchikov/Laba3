using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab_Test
{
    using Times = List<List<long>>;
    using DirectiveTimes = List<long>;

    public class Node
    {
        public List<int> FreeOrder = new();
        public List<int> CloseOrder  = new();

        public long T;
        public int Failed;
        public long B;
        public long H;

        public Node Copy()
        {
            return new Node
            {
                FreeOrder = new List<int>(FreeOrder),
                CloseOrder = new List<int>(CloseOrder),
                T = this.T,
                Failed = this.Failed,
                B = this.B,
                H = this.H
            };
        }
    }

    public class BranchBoundMethod
    {
        private readonly Times _times;
        private readonly DirectiveTimes _directiveTimes;
        private readonly int _n; 
        private readonly IHighScore _highScore;
        private readonly ILowScore _lowScore;
        private readonly IBranching _branching;

        public BranchBoundMethod(Times times, DirectiveTimes directiveTimes, IHighScore highScore, ILowScore lowScore, IBranching branching)
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

        private Node UpdateLeaf(Node node, int a)
        {
            node.T = node.T + _times[node.CloseOrder[^1]][a];
            node.CloseOrder.Add(a);
            if (node.T > _directiveTimes[a]) node.Failed++;
            if (node.FreeOrder.Count > 0)
            {
                var idx = node.FreeOrder.IndexOf(a);
                if (idx >= 0) node.FreeOrder.RemoveAt(idx);
            }
            node.B = _highScore.GetScoreB(node, _times, _directiveTimes);
            node.H = _lowScore.GetScoreH(node, _times, _directiveTimes);
            return node;
        }

        public Node BranchAndBound(out int leavesTraversed)
        {
            var root = new Node
            {
                CloseOrder = new List<int> { 0 },
                T = 0,
                Failed = 0
            };
            root.FreeOrder = Enumerable.Range(0, _n).Where(i => i != 0).ToList();
            root.H = _lowScore.GetScoreH(root, _times, _directiveTimes);
            root.B = _highScore.GetScoreB(root, _times, _directiveTimes);
            var nodes = new List<Node> { root };
            leavesTraversed = 1;
            while (true)
            {
                int nodesID = _branching.Branch(nodes);
                var nodeSelect = nodes[nodesID];
                var candidates = nodeSelect.FreeOrder.ToList();
                foreach (var a in candidates)
                {
                    nodes.Add(UpdateLeaf(nodeSelect.Copy(), a));
                    leavesTraversed++;
                }
                nodes.RemoveAt(nodesID);

                int minBId = -1;
                long minB = long.MaxValue;
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i].B < minB)
                    {
                        minB = nodes[i].B;
                        minBId = i;
                    }
                }

                for (int i = 0; i < nodes.Count; i++)
                {
                    if (i == minBId) continue;
                    if (minB <= nodes[i].H)
                    {
                        nodes.RemoveAt(i);
                        if (i < minBId) minBId--;
                        i--;
                    }
                }

                if (nodes.Count == 1 && nodes[0].B == nodes[0].H)
                {
                    var nodeFinal = nodes[0];
                    var output = nodeFinal.Copy();
                    output.CloseOrder.AddRange(output.FreeOrder);
                    return output;
                }
            }
        }
    }
}
