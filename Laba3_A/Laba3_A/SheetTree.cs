public class SheetTree
{
    public static int CreatedLeafCount = 0; 
    public DataTask Data;
    public List<int> BakedData;
    public List<int> OpenData;
    public float LowScore;
    public float HightScore;
    public SheetTree(DataTask data, List<int> bakedData, List<int> openData)
    {
        Data = data;
        BakedData = bakedData;
        OpenData = openData;
        LowScore = BranchBoundMethod._lowScore.GetScore(this);
        HightScore = BranchBoundMethod._highScore.GetScore(this);
        CreatedLeafCount++; 
    }
    public List<SheetTree> GetNextGrop()
    {
        var children = new List<SheetTree>();
        foreach (var order in OpenData)
        {
            var newBakedData = new List<int>(BakedData) { order };
            var newOpenData = new List<int>(OpenData);
            newOpenData.Remove(order);
            var childNode = new SheetTree(this.Data, newBakedData, newOpenData);
            children.Add(childNode);
        }
        return children;
    }
}
