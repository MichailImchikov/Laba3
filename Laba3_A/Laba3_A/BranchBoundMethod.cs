class BranchBoundMethod
{
    private ABranching _branching;
    private ILowScore _lowScore;
    private IHighScore _highScore;
    private List<SheetTree> _sheetTree = new();
    public BranchBoundMethod(ABranching branching, ILowScore lowScore, IHighScore highScore)
    {
        _branching = branching;
        _lowScore = lowScore;
        _highScore = highScore;
    }
    public DataDecision GetDecisionn(DataTask data)
    {
        var startSheet = new SheetTree
        {
            Data = data,
            BakedData = new List<int>(),
            OpenData = Enumerable.Range(1, data.CountOrders).ToList()
        };
        _sheetTree.Add(startSheet);
        return Run();
    }
    private DataDecision Run()
    {
        while(!CheckStopCondition())
        {
            _sheetTree = _branching.Branching(_sheetTree);
        }
        return CreateNewDataDecision();
    }
    private bool CheckStopCondition()
    {
        if (_sheetTree.Count > 1) return false;
        return _lowScore.GetScore(_sheetTree[0]) == _highScore.GetScore(_sheetTree[0]);
    }
    private DataDecision CreateNewDataDecision()
    {
        var buffList = new List<int>(_sheetTree[0].BakedData);
        buffList.AddRange(_sheetTree[0].OpenData);
        return new DataDecision
        {
            Perest = buffList
        };
    }
}

