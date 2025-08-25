using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq; // For JSONFilter

namespace ChunkProcessingSystem
{
  
    public interface IStreamFilter<T>
    {
        bool ShouldAllow(T item);
    }

   
    public class KeywordFilter : IStreamFilter<string>
    {
        private readonly string _keyword;

        public KeywordFilter(string keyword)
        {
            _keyword = keyword;
        }

        public bool ShouldAllow(string input) => input.Contains(_keyword, StringComparison.OrdinalIgnoreCase);
    }


    public class ContentTypeFilter : IStreamFilter<HttpResponseMessage>
    {
        public bool ShouldAllow(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode) return false;

            var contentType = response.Content.Headers.ContentType?.MediaType;

            // Example filter conditions
            if (contentType == "application/json")
            {
                return true; // allow JSON
            }
            else if (contentType == "text/html")
            {
                return false; // reject HTML
            }

            // Custom header check
            if (response.Headers.Contains("X-Custom-Status"))
            {
                string? customStatus = response.Headers.GetValues("X-Custom-Status").FirstOrDefault();
                return customStatus == "processed";
            }

            return false;
        }
    }

    public class JSONFilter
    {
        private readonly string _json;

        public JSONFilter(string json)
        {
            _json = json;
        }

      
        public string FilterByFields(string[] fields)
        {
            JObject parsed = JObject.Parse(_json);
            JObject filtered = new JObject();

            foreach (var field in fields)
            {
                if (parsed.TryGetValue(field, out JToken? token))
                {
                    filtered[field] = token;
                }
            }

            return filtered.ToString();
        }
    }

    public class TimeframeFilter : IStreamFilter<string>
    {
        private readonly TokenBucket _bucket;
        private readonly DateTime _allowedStartTime;

        public TimeframeFilter(TokenBucket bucket, DateTime allowedStartTime)
        {
            _bucket = bucket;
            _allowedStartTime = allowedStartTime;
        }

        public bool ShouldAllow(string input)
        {
            if (!IsWithinTimeFrame())
                return false; // outside of allowed window

            try
            {
                _bucket.UseToken();
                return true;
            }
            catch (NoTokensAvailableException)
            {
                return false;
            }
        }

        private bool IsWithinTimeFrame()
        {
            return DateTime.UtcNow >= _allowedStartTime;
        }
    }

    
    public class TokenBucket
    {
        private int _tokens;
        private readonly int _capacity;
        private readonly TimeSpan _refillInterval;
        private DateTime _lastRefill;

        public TokenBucket(int capacity, TimeSpan refillInterval)
        {
            _capacity = capacity;
            _tokens = capacity;
            _refillInterval = refillInterval;
            _lastRefill = DateTime.UtcNow;
        }

        public void UseToken()
        {
            Refill();
            if (_tokens > 0)
            {
                _tokens--;
            }
            else
            {
                throw new NoTokensAvailableException();
            }
        }

        private void Refill()
        {
            var now = DateTime.UtcNow;
            var intervalsPassed = (int)((now - _lastRefill).TotalMilliseconds / _refillInterval.TotalMilliseconds);
            if (intervalsPassed > 0)
            {
                _tokens = Math.Min(_capacity, _tokens + intervalsPassed);
                _lastRefill = now;
            }
        }
    }

    public class NoTokensAvailableException : Exception
    {
        public NoTokensAvailableException() : base("No tokens available in the bucket.") { }
    }
}
