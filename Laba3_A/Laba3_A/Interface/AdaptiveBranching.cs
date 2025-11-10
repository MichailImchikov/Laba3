public class AdaptiveBranching : ABranching
{
    protected override SheetTree GetMinScoreNode(List<SheetTree> nodes)
    {
        if (nodes == null || nodes.Count == 0)
            throw new System.ArgumentException("nodes must be a non-empty list", nameof(nodes));

        // Отбираем только неполные узлы
        var candidates = nodes.Where(n => n.OpenData.Count > 0).ToList();
        if (candidates.Count == 0) return nodes[0];

        SheetTree best = null;
        float bestScore = float.MaxValue;
        int bestSafe = -1;
        float bestGap = float.MaxValue;
        int bestBranchFactor = -1;

        foreach (var node in candidates)
        {
            // Основный критерий: минимальный верхний предел
            float upper = node.HightScore;
            float lower = node.LowScore;
            float gap = upper - lower;

            // Оценка "безопасных" заказов (которые можно добавить без штрафа, если их поставить следующими)
            int safeOrders = 0;
            foreach (var order in node.OpenData)
            {
                var sequence = new List<int>(node.BakedData) { order };
                if (node.Data.W(order, sequence.ToArray()) == 0) safeOrders++;
            }

            int branchFactor = node.OpenData.Count;

            bool better = false;
            if (upper < bestScore) better = true; // меньше верхний предел
            else if (upper == bestScore && safeOrders > bestSafe) better = true; // больше безопасных расширений
            else if (upper == bestScore && safeOrders == bestSafe && branchFactor > bestBranchFactor) better = true; // больший потенциал ветвления
            else if (upper == bestScore && safeOrders == bestSafe && branchFactor == bestBranchFactor && gap < bestGap) better = true; // меньшая неопределенность

            if (better)
            {
                best = node;
                bestScore = upper;
                bestSafe = safeOrders;
                bestGap = gap;
                bestBranchFactor = branchFactor;
            }
        }

        return best ?? candidates[0];
    }
}
