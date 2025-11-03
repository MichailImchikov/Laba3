

class BaseLowScore : ILowScore
{
    public float GetLowScore(SheetTree node)
    {
        var sumBakeOrder = node.BakedData.Select(x => node.Data.Z(x, node.BakedData.ToArray())).Sum();
        foreach (var oder in node.OpenData)
        {
            var bufBakeOrders = new List<int>(node.BakedData);
            bufBakeOrders.Add(oder);
            sumBakeOrder += node.Data.Z(oder, bufBakeOrders.ToArray());
        }
        return sumBakeOrder;
    }
}


