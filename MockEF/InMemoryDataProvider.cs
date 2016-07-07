using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MockEF
{
    public class InMemoryDataProvider
    {
        private readonly ConcurrentDictionary<Type, List<object>> _data = new ConcurrentDictionary<Type, List<object>>();

        public List<T> Get<T>()
        {
            return Get(typeof(T)).Cast<T>().ToList();
        }

        public List<object> Get(Type type)
        {
             return _data.GetOrAdd(type, new List<object>());
        }

        public void Add<T>(T obj)
        {
            Add(typeof(T), obj);
        }

        public void Add(Type type, object obj)
        {
            var dataSource = Get(type);

            _data.AddOrUpdate(type, new List<object> { obj },
                (k, v) =>
                {
                    //v.Add(obj);
                    return v;
                });
            dataSource.Add(obj);
        }

        public void AddRange<T>(IEnumerable<T> collection)
        {
            foreach (var obj in collection)
            {
                Add(obj);
            }    
        }

        public void Remove<T>(T item)
        {
            Remove(typeof (T), item);
        }

        public void Remove(Type type, object item)
        {
            var current = Get(type);
            current.Remove(item);
            _data.TryUpdate(type, current, current);
        }
    }
}
