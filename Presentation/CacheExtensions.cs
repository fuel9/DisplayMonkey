using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace DisplayMonkey
{
    public static class CacheExtensions
    {
        private static readonly object _cacheLock = new object();

        public static T GetOrAddAbsolute<T>(this Cache cache, string key, Func<T> action, DateTime when)
        {
            if (when <= DateTime.Now)
            {
                return action();
            }

            T result;
            var data = cache[key]; // Can't cast using as operator as T may be a value type

            if (data == null)
            {
                lock (_cacheLock)
                {
                    data = cache[key];

                    if (data == null)
                    {
                        result = action();

                        if (result == null)
                            return result;

                        cache.Insert(key, result, null, when, Cache.NoSlidingExpiration);
                    }
                    else
                        result = (T)data;
                }
            }
            else
                result = (T)data;

            return result;
        }

        public static T GetOrAddSliding<T>(this Cache cache, string key, Func<T> action, TimeSpan after)
        {
            if (after.TotalSeconds <= 0)
            {
                return action();
            }

            T result;
            var data = cache[key]; // Can't cast using as operator as T may be a value type

            if (data == null)
            {
                lock (_cacheLock)
                {
                    data = cache[key];

                    if (data == null)
                    {
                        result = action();

                        if (result == null)
                            return result;

                        cache.Insert(key, result, null, Cache.NoAbsoluteExpiration, after);
                    }
                    else
                        result = (T)data;
                }
            }
            else
                result = (T)data;

            return result;
        }
    }
}