public class SmartHighScore : IHighScore
{
    public float GetScore(SheetTree node)
    {
        var data = node.Data;
        var sequence = new List<int>(node.BakedData);
        var open = new List<int>();
        for (int i = 1; i <= data.CountOrders; i++)
        {
            if (!sequence.Contains(i)) open.Add(i);
        }

        // Полная перестановка уже построена
        if (open.Count == 0)
        {
            return data.CalculateCriterion(sequence.ToArray());
        }

        // Инициализация текущего положения и времени прибытия
        int currentLocation;
        int currentTime;
        if (sequence.Count == 0)
        {
            currentLocation = 0; // стартовая точка
            currentTime = 0;
        }
        else
        {
            currentLocation = sequence[^1];
            currentTime = data.Z(currentLocation, sequence.ToArray());
        }

        var plannedTail = new List<int>();

        // Строим хвост последовательности, стремясь минимизировать количество просроченных заказов
        while (open.Count > 0)
        {
            int chosen = -1;
            bool chosenIsLate = true; // предпочитаем не опаздывающие заказы
            int chosenSafeNext = -1;  // максимизируем число потенциально безопасных продолжений
            int chosenDue = int.MaxValue;
            int chosenOverflow = int.MaxValue; // если все поздние — минимизируем перерасход срока
            int chosenTravel = int.MaxValue;

            foreach (var order in open)
            {
                int travel = data.TransitionMatrix[currentLocation, order];
                int arrival = currentTime + travel;
                int due = data.DirectiveTime[order - 1];
                bool willBeLate = arrival > due;

                // Оцениваем потенциал следующего шага: сколько заказов можно будет выполнить вовремя, поставив их сразу после текущего кандидата
                int safeNextCount = 0;
                foreach (var next in open)
                {
                    if (next == order) continue;
                    int arrNext = arrival + data.TransitionMatrix[order, next];
                    if (arrNext <= data.DirectiveTime[next - 1]) safeNextCount++;
                }

                bool better = false;
                if (chosen == -1)
                {
                    better = true;
                }
                else
                {
                    // 1) предпочитаем не опаздывающих
                    if (willBeLate != chosenIsLate) better = !willBeLate;
                    else
                    {
                        // 2) максимизируем безопасные продолжения
                        if (safeNextCount != chosenSafeNext) better = safeNextCount > chosenSafeNext;
                        else
                        {
                            if (!willBeLate)
                            {
                                // 3а) среди "вовремя" — по наименьшему сроку (EDD) и затем по кратчайшему перемещению
                                if (due != chosenDue) better = due < chosenDue;
                                else if (travel != chosenTravel) better = travel < chosenTravel;
                            }
                            else
                            {
                                // 3б) среди поздних — по минимальному перерасходу и затем по кратчайшему перемещению
                                int overflow = arrival - due;
                                if (overflow != chosenOverflow) better = overflow < chosenOverflow;
                                else if (travel != chosenTravel) better = travel < chosenTravel;
                            }
                        }
                    }
                }

                if (better)
                {
                    chosen = order;
                    chosenIsLate = willBeLate;
                    chosenSafeNext = safeNextCount;
                    chosenDue = due;
                    chosenOverflow = Math.Max(0, arrival - due);
                    chosenTravel = travel;
                }
            }

            // Добавляем выбранный заказ
            plannedTail.Add(chosen);
            sequence.Add(chosen);
            open.Remove(chosen);

            // Обновляем состояние
            currentTime += chosenTravel;
            currentLocation = chosen;
        }

        // Обновим `OpenData` узла порядком, которым мы достраивали оставшиеся заказы
        node.OpenData = plannedTail;

        return node.Data.CalculateCriterion(sequence.ToArray());
    }
}
