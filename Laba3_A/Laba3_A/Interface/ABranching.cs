
public abstract class ABranching
{
    public List<SheetTree> Branching(List<SheetTree> nodes)
    {
        var delSheet = GetMinScoreNode(nodes);
        var childrens = GetShildren(delSheet);
        var buffList = new List<SheetTree>(nodes);
        buffList.AddRange(childrens);
        return buffList;
    }
    private List<SheetTree> GetShildren(SheetTree delSheet)
    {
        var children = new List<SheetTree>();
        foreach (var order in delSheet.OpenData)
        {
            var newBakedData = new List<int>(delSheet.BakedData) { order };
            var newOpenData = new List<int>(delSheet.OpenData);
            newOpenData.Remove(order);
            var childNode = new SheetTree
            {
                Data = delSheet.Data,
                BakedData = newBakedData,
                OpenData = newOpenData
            };
            children.Add(childNode);
        }
        return children;
    }
    protected abstract SheetTree GetMinScoreNode(List<SheetTree> nodes);
}

