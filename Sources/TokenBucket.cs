using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Bucket
{
    public class Token
    {
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
    }

    public class TokenBucket
    {
        private readonly BlockingCollection<Token> _tokens;
        private readonly int _capacity;
        private readonly TimeSpan _refillInterval;
        private Timer _refillTimer;

        public TokenBucket(int capacity, TimeSpan refillInterval)
        {
            _capacity = capacity;
            _refillInterval = refillInterval;
            _tokens = new BlockingCollection<Token>(new ConcurrentQueue<Token>(), capacity);

            // Start with a full bucket
            for (int i = 0; i < capacity; i++)
            {
                _tokens.Add(new Token());
            }

            // Refill periodically
            _refillTimer = new Timer(_ => Refill(), null, refillInterval, refillInterval);
        }

        /// <summary>
        /// Try to consume a token (returns true if allowed).
        /// </summary>
        public bool TryConsume()
        {
            return _tokens.TryTake(out _);
        }

        private void Refill()
        {
            while (_tokens.Count < _capacity)
            {
                _tokens.Add(new Token());
            }
        }
    }
}
