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

        public ContextBuilder<TContext> Setup<T>(Function<TContext, IDbSet<T>> action, List<T> seedData = null) where T :class, new()
        {
            MockDbSet(action);

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

        public TContext GetContext() 
        {
            return _dataContext;
        }

        private IDbSet<T> GetDbSetTestDouble<T>(IList<T> list) where T : class, new()
        {
            var dbSet = ListToDbSet(list);
            
            MockAddMethod(dbSet);
            MockAttachMethod(dbSet);
            MockCreateMethod(dbSet);
            MockRemoveMethod(dbSet);
            MockFindMethod(dbSet);

            return dbSet;
        }

        private void MockAddMethod<T>(IDbSet<T> dbSet) where T : class, new()
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

        private void MockAttachMethod<T>(IDbSet<T> dbSet) where T : class, new()
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

        private static void MockCreateMethod<T>(IDbSet<T> dbSet) where T : class, new()
        {
            dbSet.Stub(m => m.Create()).WhenCalled(x =>
            {
                //TODO: need to set identity column where appropriate.
                x.ReturnValue = new T();
            }).Return(default(T));
        }

        private void MockRemoveMethod<T>(IDbSet<T> dbSet) where T : class, new()
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

        private void MockFindMethod<T>(IDbSet<T> dbSet) where T : class, new()
        {
            dbSet.Stub(x => x.Find(Arg<object[]>.Is.Anything)).WhenCalled(x =>
            {
                var args = ((object[]) x.Arguments[0]).ToList();
                var source = _data.GetOrAdd(typeof (T), new List<object>());
                var dbset = ListToDbSet(source);
                T result = null;

                foreach (var record in dbset)
                {
                    var props = typeof (T).GetProperties();
                    var keys = props.Where(prop => Attribute.IsDefined(prop, typeof (KeyAttribute)));
                    var values = keys.Select(k => k.GetValue(record)).ToList();

                    args.OrderBy(o=>o);
                    values.OrderBy(o => o);
                    result = (T) record;

                    if (args.Any(arg => !values.ElementAt(args.IndexOf(arg)).Equals(arg)))
                    {
                        result = null;
                    }

                    if (result != null)
                    {
                        break;
                    }
                }

                x.ReturnValue = result;
            }).Return(default(T));
        }

        private IDbSet<T> ListToDbSet<T>(IList<T> data) where T : class
        {
            var queryable = data.AsQueryable();

            var dbSet = MockRepository.GenerateMock<IDbSet<T>, IQueryable>();

            dbSet.Stub(m => m.Provider).Return(queryable.Provider);
            dbSet.Stub(m => m.Expression).Return(queryable.Expression);
            dbSet.Stub(m => m.ElementType).Return(queryable.ElementType);
            dbSet.Stub(m => m.GetEnumerator()).Return(queryable.GetEnumerator());

            return dbSet;
        }

        private void MockDbSet<T>(Function<TContext, IDbSet<T>> action) where T : class, new()
        {
            _dataContext.Stub(action)
                .WhenCalled(x =>
                {
                    x.ReturnValue =
                        GetDbSetTestDouble(_data.GetOrAdd(typeof (T), new List<object>()).Select(z => (T) z).ToList());

                }).Return(default(IDbSet<T>));
        }
    }
}
