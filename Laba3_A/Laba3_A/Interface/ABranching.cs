public abstract class ABranching
{
    public HashSet<SheetTree> Branching(HashSet<SheetTree> nodes)
    {
        var delSheet = GetMinScoreNode(nodes);
        var childrens = delSheet.GetNextGrop();
        var result = new HashSet<SheetTree>(nodes);
        result.Remove(delSheet);
        foreach (var ch in childrens)
            result.Add(ch);
        return result;
    }
    protected abstract SheetTree GetMinScoreNode(HashSet<SheetTree> nodes);
}

