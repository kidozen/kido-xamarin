using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KidoZen.authentication
{
    class ConcurrentDictionary<T, U>
    {
        object sync = new object();
        Dictionary<T, U> dictionary = new Dictionary<T, U>();

        public ConcurrentDictionary()
        {
        }

        public ConcurrentDictionary(IEqualityComparer<T> comparer)
        {
            dictionary = new Dictionary<T, U>(comparer);
        }

        public U AddOrUpdate(T key, U addValue, Func<T, U, U> updateValueFactory)
        {
            lock (sync)
            {
                U value = default(U);
                if (dictionary.TryGetValue(key, out value))
                {
                    U newValue = updateValueFactory(key, value);
                    dictionary[key] = newValue;
                    return newValue;
                }
                dictionary.Add(key, addValue);
            }
            return addValue;
        }

        public bool TryGetValue(T key, out U value)
        {
            lock (sync) return dictionary.TryGetValue(key, out value);
        }


        public bool TryRemove(T key, out U value)
        {
            lock (sync)
            {
                var result = dictionary.TryGetValue(key, out value);
                if (result) dictionary.Remove(key);
                return result;
            }
        }
        
        public IEnumerable<T> Keys
        {
            get { lock (sync) return dictionary.Keys.ToArray(); }
        }
    }
}
