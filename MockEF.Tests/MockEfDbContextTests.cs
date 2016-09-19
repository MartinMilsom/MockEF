using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockEF.Tests.Data;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace MockEF.Tests
{
    public class MockEfDbContextTests
    {
        internal void SeededDataIsFound(ContextBuilderBase<IMyContext> builder)
        {
            builder.Setup(x => x.Authors, new List<Author> { new Author { Name = "bob" } })
                .Setup(x => x.Books, new List<Book>());
            var target = builder.GetContext();

            var data = target.Authors.Where(x => x.Name == "bob");

            Assert.AreEqual(1, data.Count());
        }

        internal async Task SeededDataIsFoundAsync(ContextBuilderBase<IMyContext> builder)
        {
            builder.Setup(x => x.Authors, new List<Author> { new Author { Name = "bob" } })
                .Setup(x => x.Books, new List<Book>());
            var target = builder.GetContext();

            var data = await target.Authors.Where(x => x.Name == "bob").ToListAsync();

            Assert.AreEqual(1, data.Count);
        }

        internal void DeletedSeedData_StaysRemoved(ContextBuilderBase<IMyContext> builder) 
        {
            builder.Setup(x => x.Authors, new List<Author> { new Author { Name = "bob" } })
                .Setup(x => x.Books, new List<Book>());
            var target = builder.GetContext();

            Assert.AreEqual(1, target.Authors.Count());
            Assert.IsTrue(target.Authors.Single().Name == "bob");

            target.Authors.Remove(target.Authors.Single(x => x.Name == "bob"));
            Assert.AreEqual(0, target.Authors.Count());
        }

        internal void FindOnSeededData_WithCompositeKey_ReturnsValue(ContextBuilderBase<IMyContext> builder)
        {
            builder.Setup(x => x.Authors, new List<Author>())
                .Setup(x => x.Books, new List<Book>
                {
                    new Book { Title = "Lotr", PublisherName = "pub1" ,ReleaseYear = 1954, AuthorId = 1},
                    new Book { Title = "HP", PublisherName = "pub1" ,ReleaseYear = 1997, AuthorId = 2}
                });
            var target = builder.GetContext();

            Assert.AreEqual(2, target.Books.Count());

            var result = target.Books.Find("Lotr", "pub1", 1954, 1);
            Assert.IsNotNull(result);
            Assert.AreEqual("Lotr", result.Title);
        }

        internal void FindOnSeededData_ReturnsValue(ContextBuilderBase<IMyContext> builder)
        {
            var target = builder.Setup(x => x.Authors, new List<Author> { new Author { Id = 1, Name = "bob" }, new Author { Id = 2, Name = "dave" } })
               .Setup(x => x.Books).GetContext();

            Assert.AreEqual(2, target.Authors.Count());

            var result = target.Authors.Find(2);
            Assert.IsNotNull(result);
            Assert.AreEqual("dave", result.Name);
        }

        internal void NewDynamicallyAddedData_StaysPersistedMethod(ContextBuilderBase<IMyContext> builder)
        {
            builder.Setup(x => x.Authors, new List<Author>())
            .Setup(x => x.Books, new List<Book>());
            var target = builder.GetContext();

            Assert.AreEqual(0, target.Authors.Count());
            target.Authors.Add(new Author { Name = "bob" });
            Assert.AreEqual(1, target.Authors.Count());
            Assert.IsTrue(target.Authors.Single().Name == "bob");
        }
    }
}
