public class BaseBranching : ABranching
{
    private IHighScore _score;
    public BaseBranching(IHighScore Score)
    {
        _score = Score;
    }
    protected override SheetTree GetMinScoreNode(List<SheetTree> nodes)
    {
        if (nodes == null || nodes.Count == 0)
            throw new System.ArgumentException("nodes must be a non-empty list", nameof(nodes));

        var minNode = nodes[0];
        var minScore = _score.GetScore(minNode);

        for (int i = 1; i < nodes.Count; i++)
        {
            var currentNode = nodes[i];
            var currentScore = _score.GetScore(currentNode);
            if (currentScore < minScore)
            {
                minScore = currentScore;
                minNode = currentNode;
            }
        }

        return minNode;
    }
}

