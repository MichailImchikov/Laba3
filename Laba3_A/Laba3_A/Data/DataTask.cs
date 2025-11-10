public class DataTask
{
    public int CountOrders;
    public List<int> DirectiveTime = new();
    public int[,] TransitionMatrix;

    public DataTask(int countOrders, List<int> directiveTime, int[,] transitionMatrix)
    {
        CountOrders = countOrders;
        DirectiveTime = directiveTime;
        TransitionMatrix = transitionMatrix;
    }

    // Возвращает позицию заказа i в перестановке x, либо -1 если не найден
    public int Y(int i, int[] x) // i - номер заказа, x - порядок выполнения заказов
    {
        if (x == null) return -1;
        for (int position = 0; position < x.Length; position++)
        {
            if (x[position] == i) return position;
        }
        return -1;
    }

    // Время прибытия к i-му заказу при порядке x
    public int Z(int i, int[] x) // время выполнения i -го заказа при порядке x
    {
        if (x == null || x.Length == 0)
            throw new ArgumentException("Перестановка не должна быть пустой", nameof(x));
        int yi = Y(i, x);
        if (yi < 0)
            throw new ArgumentOutOfRangeException(nameof(i), "Заказ не найден в перестановке");

        int time = TransitionMatrix[0, x[0]];
        for (int index = 0; index < yi; index++)
        {
            time += TransitionMatrix[x[index], x[index + 1]];
        }
        return time;
    }

    // Штраф за просрочку i-го заказа при порядке x (бинарный: 1 если просрочен, иначе 0)
    public int W(int i, int[] x)
    {
        if (i < 1 || i > CountOrders)
            throw new ArgumentOutOfRangeException(nameof(i));
        if (DirectiveTime == null || DirectiveTime.Count < i)
            throw new InvalidOperationException("Недостаточно директивных сроков для расчёта штрафа");

        int deliveryTime = Z(i, x);
        return deliveryTime > DirectiveTime[i - 1] ? 1 : 0;
    }

    // Быстрый расчёт критерия: одно прохождение по перестановке
    public int CalculateCriterion(int[] currentOrder)
    {
        if (currentOrder == null || currentOrder.Length == 0)
            return 0;

        int n = currentOrder.Length;
        int sum = 0;

        // Время прибытия к первому заказу
        int time = TransitionMatrix[0, currentOrder[0]];
        int order0 = currentOrder[0];
        if (order0 < 1 || order0 > CountOrders)
            throw new ArgumentOutOfRangeException(nameof(currentOrder), "Неверный идентификатор заказа в перестановке");
        if (DirectiveTime == null || DirectiveTime.Count < order0)
            throw new InvalidOperationException("Недостаточно директивных сроков для расчёта критерия");
        if (time > DirectiveTime[order0 - 1]) sum++;

        // Последовательно накапливаем время и оцениваем штрафы
        for (int pos = 0; pos < n - 1; pos++)
        {
            int from = currentOrder[pos];
            int to = currentOrder[pos + 1];
            if (to < 1 || to > CountOrders)
                throw new ArgumentOutOfRangeException(nameof(currentOrder), "Неверный идентификатор заказа в перестановке");

            time += TransitionMatrix[from, to];
            if (DirectiveTime.Count < to)
                throw new InvalidOperationException("Недостаточно директивных сроков для расчёта критерия");

            if (time > DirectiveTime[to - 1]) sum++;
        }

        return sum;
    }
}

