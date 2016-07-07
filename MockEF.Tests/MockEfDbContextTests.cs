using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockEF.Tests.Data;

namespace MockEF.Tests
{
    [TestClass]
    public class MockEfDbContextTests
    {
        [TestMethod]
        public void SeededDataIsAdded()
        {
            var builder = new Double<IMyContext>()
                .Setup(x => x.Authors, new List<Author> { new Author { Name = "bob" } })
                .Setup(x => x.Books, new List<Book>());
            var target = builder.Build();

            var data = target.Authors.Where(x => x.Name == "bob");

            Assert.AreEqual(1, data.Count());
        }

        [TestMethod]
        public void AddNewData()
        {
            var builder = new Double<IMyContext>()
                .Setup(x => x.Authors, new List<Author>())
                .Setup(x => x.Books, new List<Book>());
            var target = builder.Build();

            Assert.AreEqual(0, target.Authors.Count());
            target.Authors.Add(new Author { Name = "bob" });
            Assert.AreEqual(1, target.Authors.Count());
            Assert.IsTrue(target.Authors.Single().Name == "bob");
        }

        [TestMethod]
        public void FindData()
        {
            var target = new Double<IMyContext>()
                .Setup(x => x.Authors, new List<Author> { new Author { Id = 1, Name = "bob" }, new Author { Id = 2, Name = "dave" } })
                .Setup(x => x.Books).Build();

            Assert.AreEqual(2, target.Authors.Count());

            var result = target.Authors.Find(2);
            Assert.IsNotNull(result);
            Assert.AreEqual("dave", result.Name);

        }

        [TestMethod]
        public void FindData_WithCompositeKey()
        {
            var builder = new Double<IMyContext>()
                .Setup(x => x.Authors, new List<Author>())
                .Setup(x => x.Books, new List<Book>
                {
                    new Book { Title = "Lotr", PublisherName = "pub1" ,ReleaseYear = 1954, AuthorId = 1},
                    new Book { Title = "HP", PublisherName = "pub1" ,ReleaseYear = 1997, AuthorId = 2}
                });
            var target = builder.Build();

            Assert.AreEqual(2, target.Books.Count());

            var result = target.Books.Find("Lotr", "pub1", 1954, 1);
            Assert.IsNotNull(result);
            Assert.AreEqual("Lotr", result.Title);

        }

        [TestMethod]
        public void DeleteData()
        {
            var builder = new Double<IMyContext>()
                .Setup(x => x.Authors, new List<Author> {new Author {Name = "bob"}})
                .Setup(x => x.Books, new List<Book>());
            var target = builder.Build();

            Assert.AreEqual(1, target.Authors.Count());
            Assert.IsTrue(target.Authors.Single().Name == "bob");

            target.Authors.Remove(target.Authors.Single(x => x.Name == "bob"));
            Assert.AreEqual(0, target.Authors.Count());
        }

        [TestMethod]
        public void Enumerator_IndexOf()
        {
            var author = new Author {Name = "bob"};
            var builder = new Double<IMyContext>()
                .Setup(x => x.Authors, new List<Author> { author })
                .Setup(x => x.Books, new List<Book>());
            var target = builder.Build();
            
            var result = target.Authors.ToList();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, target.Authors.ToList().IndexOf(author));
        }

        [TestMethod]
        public void ElementType_CanBeChecked()
        {
            var author = new Author { Name = "bob" };
            var builder = new Double<IMyContext>()
                .Setup(x => x.Authors, new List<Author> { author })
                .Setup(x => x.Books, new List<Book>());
            var target = builder.Build();

            var result = target.Authors.ElementType;

            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(Author), result);
        }

        [TestMethod]
        public void CreateNewAndAttach()
        {
            var builder = new Double<IMyContext>()
                .Setup(x => x.Authors)
                .Setup(x => x.Books);
            var target = builder.Build();

            var newObj = target.Authors.Create();
            target.Authors.Attach(newObj);

            Assert.AreEqual(1, target.Authors.Count());
        }
    }
}
