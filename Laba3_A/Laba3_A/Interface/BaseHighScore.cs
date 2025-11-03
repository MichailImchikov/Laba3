using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laba3_A.Interface
{
    public class BaseHighScore : IHighScore
    {
        private DataTask _task;
        public BaseHighScore(DataTask task)
        {
            _task = new DataTask(task.CountOrders, task.DirectiveTime, task.TransitionMatrix);
        }

        public float GetHighScore(NodeTree node)
        {
            // Получаем текущую частичную перестановку из узла
            List<int> currentSequence = node.BakedData;
            int k = currentSequence.Count;

            // Если перестановка уже полная, вычисляем значение критерия
            if (k == _task.CountOrders)
            {
                return _task.CalculateCriterion((int[])currentSequence.ToArray());
            }

            // Создаем копию текущей последовательности для модификации
            List<int> sequence = new List<int>(currentSequence);

            // Шаг 1: Инициализация
            int j = 0;

            // Шаг 2: Жадное добавление заказов
            while (sequence.Count < _task.CountOrders)
            {
                // Получаем оставшиеся заказы
                List<int> remainingOrders = GetRemainingOrders(sequence);

                // Вычисляем текущее время выполнения до момента добавления нового заказа
                float currentTime = CalculateCurrentTime(sequence);

                // Находим лучший заказ для добавления
                int bestOrderIndex = FindBestOrder(sequence, remainingOrders, currentTime);

                // Добавляем заказ в последовательность
                sequence.Add(remainingOrders[bestOrderIndex]);
                j++;
            }

            // Возвращаем значение критерия для построенного решения
            return _task.CalculateCriterion((int[])sequence.ToArray());
        }

        private List<int> GetRemainingOrders(List<int> sequence)
        {
            List<int> remaining = new List<int>();
            for (int i = 0; i < _task.CountOrders; i++)
            {
                if (!sequence.Contains(i)) // предполагаем, что заказы нумеруются с 1
                {
                    remaining.Add(i);
                }
            }
            return remaining;
        }

        private float CalculateCurrentTime(List<int> sequence)
        {
            if (sequence.Count == 0) return 0;

            float time = 0;

            // Время выполнения первого заказа
            if (sequence.Count >= 1)
            {
                time += _task.TransitionMatrix[0, sequence[0]]; // t0x1
            }

            // Сумма времен выполнения остальных заказов
            for (int i = 0; i < sequence.Count - 1; i++)
            {
                time += _task.TransitionMatrix[sequence[i], sequence[i + 1]];
            }

            return time;
        }

        private int FindBestOrder(List<int> sequence, List<int> remainingOrders, float currentTime)
        {
            int bestIndex = -1;
            float minWeight = float.MaxValue;

            for (int i = 0; i < remainingOrders.Count; i++)
            {
                int order = remainingOrders[i];

                // Вычисляем общее время выполнения с добавлением нового заказа
                float totalTime = currentTime;

                if (sequence.Count > 0)
                {
                    // Добавляем время от последнего заказа к новому
                    totalTime += _task.TransitionMatrix[sequence[sequence.Count - 1], order];
                }
                else
                {
                    // Если последовательность пустая, добавляем время от начального момента
                    totalTime += _task.TransitionMatrix[0, order];
                }

                // Вычисляем вес
                float weight;
                if (totalTime <= _task.DirectiveTime[order])
                {
                    weight = _task.DirectiveTime[order] - totalTime;
                }
                else
                {
                    weight = float.MaxValue;
                }

                // Обновляем лучший заказ
                if (weight < minWeight)
                {
                    minWeight = weight;
                    bestIndex = i;
                }
            }

            // Если все заказы нарушают директивный срок, выбираем первый
            if (bestIndex == -1)
            {
                bestIndex = 0;
            }

            return bestIndex;
        }
    }
}
