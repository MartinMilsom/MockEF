using System;
using System.Data.Entity;
using System.Linq;
using Rhino.Mocks;
using System.Linq.Expressions;
using System.Data.Entity.Infrastructure;

namespace MockEF.Rhino
{
    public class ContextBuilder<TContext> : ContextBuilderBase<TContext> where TContext : class
    {
        public ContextBuilder() : base(MockRepository.GenerateMock<TContext>()) { }

        protected override IDbSet<T> ToDbSet<T>(IQueryable<T> queryable)
        {
            var dbSet = MockRepository.GenerateMock<IDbSet<T>, IQueryable, IDbAsyncEnumerable<T>> ();

            ((IDbAsyncEnumerable<T>)dbSet).Stub(m => m.GetAsyncEnumerator())
                .Return(new TestDbAsyncEnumerator<T>(queryable.GetEnumerator()));

            ((IQueryable)dbSet).Stub(m => m.Provider).Return(new TestDbAsyncQueryProvider<T>(queryable.Provider));
            dbSet.Stub(m => m.Expression).Return(queryable.Expression);
            dbSet.Stub(m => m.ElementType).Return(queryable.ElementType);
            dbSet.Stub(m => m.GetEnumerator()).Return(queryable.GetEnumerator());

            return dbSet;
        }

        protected override void StubDbSet<T>(TContext dataContext, Expression<Func<TContext, IDbSet<T>>> action) 
        {
            dataContext.Stub(action.Compile().Invoke)
                .WhenCalled(x =>
                {
                    x.ReturnValue = PerformStubDbSet<T>();

                }).Return(default(IDbSet<T>));
        }

        protected override void StubFindMethod<T>(IDbSet<T> dbSet)
        {
            dbSet.Stub(x => x.Find(Arg<object[]>.Is.Anything)).WhenCalled(x =>
            {
                var args = ((object[])x.Arguments[0]).ToList();
                x.ReturnValue = PerformFindMethod<T>(args);

            }).Return(default(T));
        }

        protected override void StubRemoveMethod<T>(IDbSet<T> dbSet) 
        {
            dbSet.Stub(x => x.Remove(Arg<T>.Is.Anything)).WhenCalled(x =>
            {
                x.ReturnValue = PerformRemoveMethod<T>((T)x.Arguments[0]);
            }).Return(null);
        }

        protected override void StubCreateMethod<T>(IDbSet<T> dbSet) 
        {
            dbSet.Stub(m => m.Create()).WhenCalled(x =>
            {
                //TODO: need to set identity column where appropriate.
                x.ReturnValue = PerformCreateMethod<T>();
            }).Return(default(T));
        }

        protected override void StubAddMethod<T>(IDbSet<T> dbSet)
        {
            dbSet.Stub(m => m.Add(Arg<T>.Is.Anything)).WhenCalled(x =>
            {
                x.ReturnValue = PerformAddMethod((T)x.Arguments[0]);
            }).Return(default(T));
        }

        protected override void StubAttachMethod<T>(IDbSet<T> dbSet)
        {
            dbSet.Stub(m => m.Attach(Arg<T>.Is.Anything)).WhenCalled(x =>
            {
                x.ReturnValue = PerformAttachMethod((T)x.Arguments[0]);
            }).Return(default(T));
        }
    }
}
