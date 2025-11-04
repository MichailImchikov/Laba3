using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

class BranchBoundMethod
{
    private ABranching _branching;
    static public ILowScore _lowScore;
    static public IHighScore _highScore;
    private HashSet<SheetTree> _sheetTree = new();
    public BranchBoundMethod(ABranching branching, ILowScore lowScore, IHighScore highScore)
    {
        _branching = branching;
        _lowScore = lowScore;
        _highScore = highScore;
    }
    public DataDecision GetDecisionn(DataTask data)
    {
        _sheetTree.Clear();
        var startSheet = new SheetTree(data, new List<int>(), Enumerable.Range(1, data.CountOrders).ToList());
        
        _sheetTree.Add(startSheet);
        return Run();
    }
    private DataDecision Run()
    {
        while(!CheckStopCondition())
        {

            _sheetTree = _branching.Branching(_sheetTree);
            Clipping();
        }
        return CreateNewDataDecision();
    }
    private void Clipping()
    {
        var up = _sheetTree.Min(t => t.HightScore);
        var minNode = _sheetTree.First(t => t.HightScore == up);
        var toRemove = _sheetTree.Where(t => up <= t.LowScore && !ReferenceEquals(t, minNode)).ToList();
        foreach (var node in toRemove)
        {
            _sheetTree.Remove(node);
        }
    }
    private bool CheckStopCondition()
    {
        if (_sheetTree.Count == 1)
        {
            var node = _sheetTree.First();
            if (node.LowScore == node.HightScore)
                return true;
        }
        return false;
    }
    private DataDecision CreateNewDataDecision()
    {
        var node = _sheetTree.First();
        var buffList = new List<int>(node.BakedData);
        buffList.AddRange(node.OpenData);
        return new DataDecision
        {
            Perest = buffList
        };
    }
}

