
public abstract class ABranching
{
    public List<SheetTree> Branching(List<SheetTree> nodes)
    {
        var delSheet = GetMinScoreNode(nodes);
        var childrens = delSheet.GetNextGrop();
        var buffList = new List<SheetTree>(nodes);
        buffList.Remove(delSheet);
        buffList.AddRange(childrens);
        return buffList;
    }
    protected abstract SheetTree GetMinScoreNode(List<SheetTree> nodes);
}

