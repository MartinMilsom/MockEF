using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace MockEF
{
    public abstract class ContextBuilderBase<TContext> where TContext : class
    {
        private readonly TContext _dataContext;
        private readonly ConcurrentDictionary<Type, List<object>> _data = new ConcurrentDictionary<Type, List<object>>();

        public ContextBuilderBase(TContext entityContext)
        {
            _dataContext = entityContext;
        }

        public TContext GetContext()
        {
            return _dataContext;
        }

        public ContextBuilderBase<TContext> Setup<T>(Expression<Func<TContext, IDbSet<T>>> action, List<T> seedData = null) where T : class, new()
        {
            StubDbSet(_dataContext, action);

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

        protected IDbSet<T> PerformStubDbSet<T>() where T : class, new()
        {
            return GetDbSetTestDouble(_data.GetOrAdd(typeof(T), new List<object>()).Select(z => (T)z).ToList());
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

        protected T PerformAddMethod<T>(T entity) where T : class, new()
        {
            var source = _data.GetOrAdd(entity.GetType(), new List<object>());
            source.Add(entity);
            _data.TryUpdate(entity.GetType(), source, source);
            return entity;
        }

        protected T PerformAttachMethod<T>(T entity) where T : class, new()
        {
            //TODO: mimicking "add" for now, this doesnt happen in reality. 
            return PerformAddMethod(entity);
        }

        protected T PerformCreateMethod<T>() where T : class, new()
        {
            return new T();
        }

        protected T PerformRemoveMethod<T>(T entity)
        {
            var source = _data.GetOrAdd(entity.GetType(), new List<object>());
            source.Remove(entity);
            _data.TryUpdate(entity.GetType(), source, source);
            return entity;
        }

        protected T PerformFindMethod<T>(List<object> findValues) where T : class, new()
        {
            var source = _data.GetOrAdd(typeof(T), new List<object>());
            var dbset = ToDbSet(source);

            foreach (var record in dbset)
            {
                var keys = typeof(T).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(KeyAttribute)));
                var values = keys.Select(k => k.GetValue(record)).ToList();

                findValues.OrderBy(o => o);
                values.OrderBy(o => o);

                if (findValues.All(arg => values.ElementAt(findValues.IndexOf(arg)).Equals(arg)))
                {
                    return (T)record;
                }
            }
            return null;
        }

        private IDbSet<T> ToDbSet<T>(IEnumerable<T> data) where T : class
        {
            var queryable = data.AsQueryable();

            return ToDbSet(queryable);
        }

        protected abstract void StubDbSet<T>(TContext dataContext, Expression<Func<TContext, IDbSet<T>>> action) where T : class, new();
        protected abstract IDbSet<T> ToDbSet<T>(IQueryable<T> queryable) where T : class;
        protected abstract void StubFindMethod<T>(IDbSet<T> dbSet) where T : class, new();
        protected abstract void StubRemoveMethod<T>(IDbSet<T> dbSet) where T : class, new();
        protected abstract void StubCreateMethod<T>(IDbSet<T> dbSet) where T : class, new();
        protected abstract void StubAddMethod<T>(IDbSet<T> dbSet) where T : class, new();
        protected abstract void StubAttachMethod<T>(IDbSet<T> dbSet) where T : class, new();
    }
}
