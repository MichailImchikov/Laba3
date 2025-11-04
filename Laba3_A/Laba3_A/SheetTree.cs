
public class SheetTree
{
    public DataTask Data;
    public List<int> BakedData;
    public List<int> OpenData;
    public List<SheetTree> GetNextGrop()
    {
        var children = new List<SheetTree>();
        foreach (var order in OpenData)
        {
            var newBakedData = new List<int>(BakedData) { order };
            var newOpenData = new List<int>(OpenData);
            newOpenData.Remove(order);
            var childNode = new SheetTree
            {
                Data = Data,
                BakedData = newBakedData,
                OpenData = newOpenData
            };
            children.Add(childNode);
        }
        return children;
    }
}
