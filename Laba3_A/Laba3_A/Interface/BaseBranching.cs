public class BaseBranching : ABranching
{
    protected override SheetTree GetMinScoreNode(List<SheetTree> nodes)
    {
        if (nodes == null || nodes.Count == 0)
            throw new System.ArgumentException("nodes must be a non-empty list", nameof(nodes));

        var minNode = nodes[0];
        var minScore = minNode.HightScore;

        for (int i = 1; i < nodes.Count; i++)
        {
            var currentNode = nodes[i];
            var currentScore = currentNode.HightScore;
            if (currentScore < minScore)
            {
                minScore = currentScore;
                minNode = currentNode;
            }
        }

        return minNode;
    }
}

