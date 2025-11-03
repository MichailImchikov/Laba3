
public class SheetTree
{
    public List<SheetTree> NextNodes = new();
    public DataTask Data;
    public List<int> BakedData;
    public List<int> OpenData;
    public List<SheetTree> GetSheets()
    {
        var listNodes = new List<SheetTree>();
        if (NextNodes.Count == 0)
        {
            listNodes.Add(this);
            return listNodes;
        }
        foreach (var node in NextNodes)
        {
            listNodes.AddRange(node.GetSheets());
        }
        return listNodes;
    }
}
