public class SheetTree : System.IEquatable<SheetTree>
{
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

    public bool Equals(SheetTree other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        // Identity is based on Data reference and baked sequence only (stable key)
        if (!ReferenceEquals(Data, other.Data)) return false;
        if (BakedData.Count != other.BakedData.Count) return false;
        for (int i = 0; i < BakedData.Count; i++)
            if (BakedData[i] != other.BakedData[i]) return false;
        return true;
    }

    public override bool Equals(object obj) => Equals(obj as SheetTree);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(Data);
            foreach (var v in BakedData) hash = hash * 23 + v.GetHashCode();
            return hash;
        }
    }
}
