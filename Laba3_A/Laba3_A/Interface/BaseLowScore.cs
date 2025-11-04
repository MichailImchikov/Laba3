class BaseLowScore : ILowScore
{
    public int GetScore(SheetTree node)
    {
        var sumBakeOrder = node.BakedData.Select(x => node.Data.W(x, node.BakedData.ToArray())).Sum();
        foreach (var oder in node.OpenData)
        {
            var bufBakeOrders = new List<int>(node.BakedData);
            bufBakeOrders.Add(oder);
            sumBakeOrder += node.Data.W(oder, bufBakeOrders.ToArray());
        }
        return sumBakeOrder;
    }
}


