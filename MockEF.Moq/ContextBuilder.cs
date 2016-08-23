using System;
using System.Linq;

namespace MockEF.Moq
{
    public class ContextBuilder<TContext> : ContextBuilderBase<TContext> where TContext : class
    {
        public ContextBuilder() : base(null) { }

        protected override void StubAddMethod<T>(System.Data.Entity.IDbSet<T> dbSet)
        {
            throw new NotImplementedException();
        }

        protected override void StubAttachMethod<T>(System.Data.Entity.IDbSet<T> dbSet)
        {
            throw new NotImplementedException();
        }

        protected override void StubCreateMethod<T>(System.Data.Entity.IDbSet<T> dbSet)
        {
            throw new NotImplementedException();
        }

        protected override void StubDbSet<T>(TContext dataContext, Func<TContext, System.Data.Entity.IDbSet<T>> action)
        {
            throw new NotImplementedException();
        }

        protected override void StubFindMethod<T>(System.Data.Entity.IDbSet<T> dbSet)
        {
            throw new NotImplementedException();
        }

        protected override void StubRemoveMethod<T>(System.Data.Entity.IDbSet<T> dbSet)
        {
            throw new NotImplementedException();
        }

        protected override System.Data.Entity.IDbSet<T> ToDbSet<T>(IQueryable<T> queryable)
        {
            throw new NotImplementedException();
        }
    }
}
