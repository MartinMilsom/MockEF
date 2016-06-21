using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace MockEF.Tests.Data
{
    public interface IMyContext
    {
        IDbSet<Author> Authors { get; set; }
        IDbSet<Book> Books { get; set; }
    }

    public partial class Author
    {
        public Author()
        {
            Books = new HashSet<Book>();
        }

        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<Book> Books { get; set; }
    }

    public partial class Book
    {
        [Key, Column(Order = 0)]
        public string Title { get; set; }

        [Key, Column(Order = 1)]
        public string PublisherName { get; set; }

        [Key, Column(Order = 2)]
        public int ReleaseYear { get; set; }

        [Key, Column(Order = 3)]
        public int AuthorId { get; set; }

        public virtual Author Author { get; set; }
    }
}
