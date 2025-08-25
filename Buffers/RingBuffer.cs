using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RingBuffer
{
    public interface IBuffer<T> : IEnumerable<T>
    {
        int Count { get; }
        int Capacity { get; }

        void Insert(T item);
        void Clear();
    }

    public class BufferWithQueue<T> : IBuffer<T>
    {
        private readonly Queue<T> queue;

        public int Count => queue.Count;
        public int Capacity { get; }
        
        // First item in the buffer
        public T First => queue.First();

        // Last item inserted into the buffer
        public T Last => queue.Last();

        public BufferWithQueue(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");

            Capacity = capacity;
            queue = new Queue<T>(capacity);
        }

        public void Insert(T item)
        {
            if (queue.Count == Capacity)
            {
                queue.Dequeue(); // Remove oldest
            }
            queue.Enqueue(item); // Add newest
        }

        public void Clear() => queue.Clear();

        public IEnumerator<T> GetEnumerator() => queue.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
