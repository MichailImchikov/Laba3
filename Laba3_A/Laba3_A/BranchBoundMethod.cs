
class BranchBoundMethod
{
    private IBranching _branching;
    private ILowScore _lowScore;
    private IHighScore _highScore;
    private List<SheetTree> _sheetTree = new();
    public BranchBoundMethod(IBranching branching, ILowScore lowScore, IHighScore highScore)
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
        Run();
    }
    private DataDecision Run()
    {
        if(CheckStopCondition()) 
    }
    private bool CheckStopCondition()
    {
        if (_sheetTree.Count > 1) return false;
        return _lowScore.GetScore(_sheetTree[0]) == _highScore.GetScore(_sheetTree[0]);
    }
    private DataDecision CreateNewDataDecision()
    {
        return new DataDecision
        {
            Perest = _sheetTree[0].BakedData.AddRange(_sheetTree[0].OpenData);
        };
    }
}

