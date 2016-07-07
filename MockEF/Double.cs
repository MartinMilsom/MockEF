using System;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;
using System.Data.Entity;
using MockEF.Interceptors;

namespace MockEF
{
    public class Double<TContext> where TContext : class
    {
        private readonly InMemoryDataProvider _dataProvider;
        private readonly Dictionary<Type, Func<object>> _callbacks = new Dictionary<Type, Func<object>>();
        private readonly TContext _context;

        public Double()
        {
            _dataProvider = new InMemoryDataProvider();
            var generator = new ProxyGenerator();
            _context = (TContext)generator.CreateInterfaceProxyWithoutTarget(typeof (TContext), new ContextInterceptor(_callbacks));
        }

        public TContext Build() 
        {
            return _context;
        }

        public Double<TContext> Setup<T>(Func<TContext, IEnumerable<T>> property, IEnumerable<T> seed = null) where T : class
        {
            var data = property(_context);
            if (data != null)
            {
                _dataProvider.AddRange(data);
            }
            if (seed != null)
            {
                _dataProvider.AddRange(seed);
            }

            _callbacks.Add(typeof (T), GetDataForType<T>);
            return this;
        }

        private IEnumerable<T> GetDataForType<T>() where T : class
        {
            var data = _dataProvider.Get<T>();
            var generator = new ProxyGenerator();
            var proxy = generator.CreateInterfaceProxyWithoutTarget(typeof (IDbSet<T>), new[] {typeof (IQueryable)},
                new DbSetInterceptor(data.AsQueryable(), _dataProvider));
            return proxy as IEnumerable<T>;
        }
    }
}
