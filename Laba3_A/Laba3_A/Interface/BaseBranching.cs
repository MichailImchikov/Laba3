public class BaseBranching : ABranching
{
    protected override SheetTree GetMinScoreNode(HashSet<SheetTree> nodes)
    {
        if (nodes == null || nodes.Count == 0)
            throw new System.ArgumentException("nodes must be a non-empty set", nameof(nodes));

        using var enumerator = nodes.GetEnumerator();
        enumerator.MoveNext();
        var minNode = enumerator.Current;
        var minScore = minNode.HightScore;

        foreach (var currentNode in nodes)
        {
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

