using System.Diagnostics;

class BranchBoundMethod
{
    private ABranching _branching;
    static public ILowScore _lowScore;
    static public IHighScore _highScore;
    private List<SheetTree> _sheetTree = new();
    public BranchBoundMethod(ABranching branching, ILowScore lowScore, IHighScore highScore)
    {
        _branching = branching;
        _lowScore = lowScore;
        _highScore = highScore;
    }
    public DataDecision GetDecisionn(DataTask data)
    {
        SheetTree.CreatedLeafCount = 0; // сброс счетчика перед задачей
        var startSheet = new SheetTree(data, new List<int>(), Enumerable.Range(1, data.CountOrders).ToList());
        _sheetTree.Clear();
        _sheetTree.AddRange(startSheet.GetNextGrop());
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
        int indexMax = _sheetTree.FindIndex(t => t.HightScore == up);
        List<int> remove = new List<int>();
        for (int index = 0; index < _sheetTree.Count; index++)
        {
            if (up <= _sheetTree[index].LowScore && indexMax != index) remove.Add(index);

        }
        remove.Sort();
        remove.Reverse();
        foreach (int index in remove)
        {
            _sheetTree.RemoveAt(index);
        }
    }
    private bool CheckStopCondition()
    {
        if (_sheetTree.Count == 1 && _sheetTree[0].LowScore == _sheetTree[0].HightScore)
            return true;
        return false;
    }
    private DataDecision CreateNewDataDecision()
    {
        var buffList = new List<int>(_sheetTree[0].BakedData);
        buffList.AddRange(_sheetTree[0].OpenData);
        return new DataDecision
        {
            Perest = buffList,
            CountLeaf = SheetTree.CreatedLeafCount // сохранение количества листьев
        };
    }
}

