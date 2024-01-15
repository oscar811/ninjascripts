using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjaTrader.Custom.Strategies.RajAlgos
{
    class UniqueStack<T>
    {
        private Stack<T> stack;
        private Dictionary<double, T> sortedDict;

        public UniqueStack()
        {
            stack = new Stack<T>();
            sortedDict = new Dictionary<double, T>();
        }

        public void Push(double key, T item)
        {
            if (!sortedDict.ContainsKey(key))
            {
                stack.Push(item);
                sortedDict.Add(key, item);
            }
        }

        public T Pop()
        {
            if (stack.Count == 0)
            {
                throw new InvalidOperationException("Stack is empty.");
            }

            var poppedItem = stack.Pop();
            var lastKey = sortedDict.Last().Key;
            sortedDict.Remove(lastKey);

            return poppedItem;
        }

        public T Peek()
        {
            if (stack.Count == 0)
            {
                throw new InvalidOperationException("Stack is empty.");
            }

            return stack.Peek();
        }

        public int Count()
        {
            return sortedDict.Count;
        }

        public IEnumerable<T> GetSortedItems()
        {
            return sortedDict.Values;
        }
    }
}
