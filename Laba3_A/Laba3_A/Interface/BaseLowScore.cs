class BaseLowScore : ILowScore
{
    public int GetScore(SheetTree node)
    {
        //var sumBakeOrder = node.BakedData.Select(x => node.Data.W(x, node.BakedData.ToArray())).Sum();
        //foreach (var oder in node.OpenData)
        //{
        //    var bufBakeOrders = new List<int>(node.BakedData);
        //    bufBakeOrders.Add(oder);
        //    sumBakeOrder += node.Data.W(oder, bufBakeOrders.ToArray());
        //}
        int currentTime = 0;
        int sum = 0;
        if (node.BakedData.Count != 0)
        {
            currentTime = node.Data.TransitionMatrix[0, node.BakedData.First()];
            sum = currentTime > node.Data.DirectiveTime[node.BakedData.First() - 1] ? 1 : 0;
            int index = 0;
            foreach (var order in node.BakedData)
            {
                if (order == node.BakedData.First()) continue;
                currentTime += node.Data.TransitionMatrix[node.BakedData[index], order];
                sum += currentTime > node.Data.DirectiveTime[order - 1] ? 1 : 0;
                index++;
            }
            foreach (var order in node.OpenData)
            {
                sum += currentTime + node.Data.TransitionMatrix[node.BakedData.Last(), order] > node.Data.DirectiveTime[order - 1] ? 1 : 0;
            }
        }
        else
        {
            foreach (var order in node.OpenData)
            {
                sum += currentTime + node.Data.TransitionMatrix[0, order] > node.Data.DirectiveTime[order - 1] ? 1 : 0;
            }
        }




        return sum;
        //return sumBakeOrder;
    }
}


