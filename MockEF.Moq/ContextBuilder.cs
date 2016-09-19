using System;
using System.Linq;
using Moq;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Data.Entity.Infrastructure;

namespace MockEF.Moq
{
    public class ContextBuilder<TContext> : ContextBuilderBase<TContext> where TContext : class
    {
        public ContextBuilder() : base(new Mock<TContext>().Object)
        {
        }

        protected override IDbSet<T> ToDbSet<T>(IQueryable<T> queryable)
        {
            var dbSet = new Mock<IDbSet<T>>();

            dbSet.As<IQueryable>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<T>(queryable.Provider));

            dbSet.As<IDbAsyncEnumerable<T>>()
                 .Setup(m => m.GetAsyncEnumerator())
                 .Returns(new TestDbAsyncEnumerator<T>(queryable.GetEnumerator()));

            dbSet.Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            return dbSet.Object;
        }

        protected override void StubDbSet<T>(TContext dataContext, Expression<Func<TContext, IDbSet<T>>> action)
        {
            var mock = Mock.Get<TContext>(dataContext);
            mock.SetupGet(action)
                .Returns(() => PerformStubDbSet<T>());
        }

        protected override void StubAddMethod<T>(IDbSet<T> dbSet)
        {
            var mock = Mock.Get(dbSet);
            mock.Setup(x => x.Add(It.IsAny<T>()))
              .Returns<T>(inputParameter => PerformAddMethod(inputParameter));
        }

        protected override void StubAttachMethod<T>(IDbSet<T> dbSet)
        {
            var mock = Mock.Get(dbSet);
            mock.Setup(x => x.Attach(It.IsAny<T>()))
             .Returns<T>(inputParameter => PerformAddMethod(inputParameter));
        }

        protected override void StubCreateMethod<T>(IDbSet<T> dbSet)
        {
            var mock = Mock.Get(dbSet);
            mock.Setup(x => x.Create())
             .Returns(() => PerformCreateMethod<T>());
        }

        protected override void StubFindMethod<T>(IDbSet<T> dbSet)
        {
            var mock = Mock.Get(dbSet);
            mock.Setup(x => x.Find(It.IsAny<object[]>()))
               .Returns<object[]>(x => PerformFindMethod<T>(x.ToList()));
        }

        protected override void StubRemoveMethod<T>(IDbSet<T> dbSet)
        {
            var mock = Mock.Get(dbSet);
            mock.Setup(x => x.Remove(It.IsAny<T>()))
              .Returns<T>(x => PerformRemoveMethod<T>(x));
        }
    }
}
