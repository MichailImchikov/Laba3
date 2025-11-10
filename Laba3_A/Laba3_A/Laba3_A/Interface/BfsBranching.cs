public class BfsBranching : ABranching
{
    protected override SheetTree GetMinScoreNode(List<SheetTree> nodes)
    {
        if (nodes == null || nodes.Count == 0)
            throw new System.ArgumentException("nodes must be a non-empty list", nameof(nodes));

        var first = nodes.FirstOrDefault(x => x.OpenData.Count > 1);
        return first;
    }
}
