using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using Rhino.Mocks;

namespace MockEF
{
    public class ContextBuilder<TContext> where TContext : class
    {
        private readonly TContext _dataContext = MockRepository.GenerateMock<TContext>();
        private readonly ConcurrentDictionary<Type, List<object>> _data = new ConcurrentDictionary<Type, List<object>>();

        public TContext GetContext()
        {
            return _dataContext;
        }

        public ContextBuilder<TContext> Setup<T>(Func<TContext, IDbSet<T>> action, List<T> seedData = null) where T : class, new()
        {
            StubDbSet(action);

            if (seedData != null)
            {
                _data.AddOrUpdate(typeof(T), seedData.Cast<object>().ToList(), (key, val) =>
                {
                    val.AddRange(seedData);
                    return val;
                });
            }
            return this;
        }

        private void StubDbSet<T>(Func<TContext, IDbSet<T>> action) where T : class, new()
        {
            _dataContext.Stub(action.Invoke)
                .WhenCalled(x =>
                {
                    x.ReturnValue =
                        GetDbSetTestDouble(_data.GetOrAdd(typeof(T), new List<object>()).Select(z => (T)z).ToList());

                }).Return(default(IDbSet<T>));
        }

        private IDbSet<T> GetDbSetTestDouble<T>(IList<T> list) where T : class, new()
        {
            var dbSet = ToDbSet(list);
            
            StubAddMethod(dbSet);
            StubAttachMethod(dbSet);
            StubCreateMethod(dbSet);
            StubRemoveMethod(dbSet);
            StubFindMethod(dbSet);

            return dbSet;
        }

        private void StubAddMethod<T>(IDbSet<T> dbSet) where T : class, new()
        {
            dbSet.Stub(m => m.Add(Arg<T>.Is.Anything)).WhenCalled(x =>
            {
                var arg = x.Arguments[0];
                var source = _data.GetOrAdd(arg.GetType(), new List<object>());
                source.Add(arg);
                _data.TryUpdate(arg.GetType(), source, source);
                x.ReturnValue = (T) arg;
            }).Return(default(T));
        }

        private void StubAttachMethod<T>(IDbSet<T> dbSet) where T : class, new()
        {
            dbSet.Stub(m => m.Attach(Arg<T>.Is.Anything)).WhenCalled(x =>
            {
                //TODO: mimicking "add" for now, this doesnt happen in reality. 
                var arg = x.Arguments[0];
                var source = _data.GetOrAdd(arg.GetType(), new List<object>());
                source.Add(arg);
                _data.TryUpdate(arg.GetType(), source, source);
                x.ReturnValue = (T) arg;
            }).Return(default(T));
        }

        private static void StubCreateMethod<T>(IDbSet<T> dbSet) where T : class, new()
        {
            dbSet.Stub(m => m.Create()).WhenCalled(x =>
            {
                //TODO: need to set identity column where appropriate.
                x.ReturnValue = new T();
            }).Return(default(T));
        }

        private void StubRemoveMethod<T>(IDbSet<T> dbSet) where T : class, new()
        {
            dbSet.Stub(x => x.Remove(Arg<T>.Is.Anything)).WhenCalled(x =>
            {
                var arg = (T) x.Arguments[0];
                var source = _data.GetOrAdd(arg.GetType(), new List<object>());
                source.Remove(arg);
                _data.TryUpdate(arg.GetType(), source, source);
                x.ReturnValue = arg;
            }).Return(null);
        }

        private void StubFindMethod<T>(IDbSet<T> dbSet) where T : class, new()
        {
            dbSet.Stub(x => x.Find(Arg<object[]>.Is.Anything)).WhenCalled(x =>
            {
                var args = ((object[]) x.Arguments[0]).ToList();
                var source = _data.GetOrAdd(typeof (T), new List<object>());
                var dbset = ToDbSet(source);

                foreach (var record in dbset)
                {
                    var keys = typeof(T).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(KeyAttribute)));
                    var values = keys.Select(k => k.GetValue(record)).ToList();

                    args.OrderBy(o=>o);
                    values.OrderBy(o => o);

                    if (args.All(arg => values.ElementAt(args.IndexOf(arg)).Equals(arg)))
                    {
                        x.ReturnValue = (T)record;
                        break;
                    }
                }
                
            }).Return(default(T));
        }

        private static IDbSet<T> ToDbSet<T>(IEnumerable<T> data) where T : class
        {
            var queryable = data.AsQueryable();

            var dbSet = MockRepository.GenerateMock<IDbSet<T>, IQueryable>();

            dbSet.Stub(m => m.Provider).Return(queryable.Provider);
            dbSet.Stub(m => m.Expression).Return(queryable.Expression);
            dbSet.Stub(m => m.ElementType).Return(queryable.ElementType);
            dbSet.Stub(m => m.GetEnumerator()).Return(queryable.GetEnumerator());

            return dbSet;
        }
    }
}
