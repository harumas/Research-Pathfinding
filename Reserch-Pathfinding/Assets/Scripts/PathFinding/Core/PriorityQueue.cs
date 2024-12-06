// https://qiita.com/SEQUEL/items/1e51d80a0ce914d44e74

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PathFinder.Core
{
    public class PriorityQueue<TKey, TValue> : IEnumerable<TValue>
    {
        private readonly List<KeyValuePair<TKey, TValue>> _data = new List<KeyValuePair<TKey, TValue>>();
        private readonly bool _isDescending;
        private readonly Func<TValue, TKey> _keySelector;
        private readonly IComparer<TKey> _keyComparer;

        public PriorityQueue(Func<TValue, TKey> keySelector, bool isDescending = true)
            : this(keySelector, Comparer<TKey>.Default, isDescending)
        {
        }

        public PriorityQueue(Func<TValue, TKey> keySelector, IComparer<TKey> keyComparer, bool isDescending = true)
        {
            _keySelector = keySelector;
            _keyComparer = keyComparer;
            _isDescending = isDescending;
        }

        private PriorityQueue(List<KeyValuePair<TKey, TValue>> data, Func<TValue, TKey> keySelector, IComparer<TKey> keyComparer,
            bool isDescending = true)
        {
            _data = data;
            _keySelector = keySelector;
            _keyComparer = keyComparer;
            _isDescending = isDescending;
        }

        public void Enqueue(TValue item)
        {
            _data.Add(new KeyValuePair<TKey, TValue>(_keySelector(item), item));
            var childIndex = _data.Count - 1;
            while (childIndex > 0)
            {
                var parentIndex = (childIndex - 1) / 2;
                if (Compare(_data[childIndex].Key, _data[parentIndex].Key) >= 0)
                    break;
                Swap(childIndex, parentIndex);
                childIndex = parentIndex;
            }
        }

        public TValue Dequeue()
        {
            var lastIndex = _data.Count - 1;
            var firstItem = _data[0];
            _data[0] = _data[lastIndex];
            _data.RemoveAt(lastIndex--);
            var parentIndex = 0;
            while (true)
            {
                var childIndex = parentIndex * 2 + 1;
                if (childIndex > lastIndex)
                    break;
                var rightChild = childIndex + 1;
                if (rightChild <= lastIndex && Compare(_data[rightChild].Key, _data[childIndex].Key) < 0)
                    childIndex = rightChild;
                if (Compare(_data[parentIndex].Key, _data[childIndex].Key) <= 0)
                    break;
                Swap(parentIndex, childIndex);
                parentIndex = childIndex;
            }

            return firstItem.Value;
        }

        public TValue Peek()
        {
            return _data[0].Value;
        }

        private void Swap(int a, int b)
        {
            (_data[a], _data[b]) = (_data[b], _data[a]);
        }

        private int Compare(TKey a, TKey b)
        {
            return _isDescending ? _keyComparer.Compare(b, a) : _keyComparer.Compare(a, b);
        }

        public int Count => _data.Count;

        public PriorityQueue<TKey, TValue> Clone()
        {
            PriorityQueue<TKey, TValue> newQueue = new PriorityQueue<TKey, TValue>(_data, _keySelector, Comparer<TKey>.Default, _isDescending);
            return newQueue;
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return _data.Select(r => r.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}