namespace Ana.Source.Utils.DataStructures
{
    using System;
    using System.Collections.Concurrent;

    internal class TTLCache<V>
    {
        private ConcurrentDictionary<V, DateTime> cache;

        public TTLCache()
        {
            this.cache = new ConcurrentDictionary<V, DateTime>();
            this.DefaultTimeToLive = TimeSpan.MaxValue;
        }

        public TTLCache(TimeSpan defaultTimeToLive) : this()
        {
            this.DefaultTimeToLive = defaultTimeToLive;
        }

        public TTLCache(TimeSpan defaultTimeToLive, TimeSpan randomTimeToLiveOffset) : this(defaultTimeToLive)
        {
            this.RandomTimeToLiveOffset = randomTimeToLiveOffset;
        }

        protected TimeSpan DefaultTimeToLive { get; set; }

        protected TimeSpan RandomTimeToLiveOffset { get; set; }

        protected static Random Random = new Random();

        public void Add(V value)
        {
            if (this.RandomTimeToLiveOffset != null)
            {
                Int32 maximumOffset = (Int32)this.RandomTimeToLiveOffset.TotalMilliseconds;
                TimeSpan offset = TimeSpan.FromMilliseconds(TTLCache<V>.Random.Next(-maximumOffset, maximumOffset));
                TimeSpan timeToLive = this.DefaultTimeToLive + offset;

                this.Add(value, timeToLive);
            }
            else
            {
                this.Add(value, this.DefaultTimeToLive);
            }
        }

        public void Add(V value, TimeSpan timeToLive)
        {
            DateTime expireTime = timeToLive == TimeSpan.MaxValue ? DateTime.MaxValue : DateTime.Now + timeToLive;

            this.cache.AddOrUpdate(value, expireTime, (key, ttl) => { return ttl; });
        }

        public Boolean Contains(V value)
        {
            if (this.cache.ContainsKey(value))
            {
                DateTime result;

                if (this.cache.TryGetValue(value, out result))
                {
                    if (DateTime.Now <= result || result == DateTime.MaxValue)
                    {
                        // Cache contains valid unexpired entry
                        return true;
                    }

                    // Cache expired
                    this.cache.TryRemove(value, out result);
                }
            }

            return false;
        }
    }

    internal class TTLCache<K, V> : TTLCache<K> where V : class
    {
        private ConcurrentDictionary<K, Tuple<V, DateTime>> cache;

        public TTLCache() : base()
        {
            this.cache = new ConcurrentDictionary<K, Tuple<V, DateTime>>();
        }

        public TTLCache(TimeSpan defaultTimeToLive) : base(defaultTimeToLive)
        {
            this.cache = new ConcurrentDictionary<K, Tuple<V, DateTime>>();
        }

        public TTLCache(TimeSpan defaultTimeToLive, TimeSpan randomTimeToLiveOffset) : base(defaultTimeToLive, randomTimeToLiveOffset)
        {
            this.cache = new ConcurrentDictionary<K, Tuple<V, DateTime>>();
        }

        public void Add(K key, V value)
        {
            if (this.RandomTimeToLiveOffset != null)
            {
                Int32 maximumOffset = (Int32)this.RandomTimeToLiveOffset.TotalMilliseconds;
                TimeSpan offset = TimeSpan.FromMilliseconds(TTLCache<K, V>.Random.Next(-maximumOffset, maximumOffset));
                TimeSpan timeToLive = this.DefaultTimeToLive + offset;

                this.Add(key, value, timeToLive);
            }
            else
            {
                this.Add(key, value, this.DefaultTimeToLive);
            }
        }

        public void Add(K key, V value, TimeSpan timeToLive)
        {
            DateTime expireTime = timeToLive == TimeSpan.MaxValue ? DateTime.MaxValue : DateTime.Now + timeToLive;
            Tuple<V, DateTime> newValue = new Tuple<V, DateTime>(value, expireTime);

            this.cache.AddOrUpdate(key, newValue, (temp, ttl) => { return ttl; });
        }

        public Boolean TryGetValue(K key, out V value)
        {
            Tuple<V, DateTime> result;
            value = null;

            if (this.cache.TryGetValue(key, out result))
            {
                if (DateTime.Now <= result.Item2 || result.Item2 == DateTime.MaxValue)
                {
                    // Cache contains valid unexpired entry
                    value = result.Item1;
                    return true;
                }

                // Cache expired
                this.cache.TryRemove(key, out result);
            }

            return false;
        }
    }
    //// End class
}
//// End namespace