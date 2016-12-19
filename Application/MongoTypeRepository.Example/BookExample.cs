using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoTypeRepository.Example.CustomRepositories;
using MongoTypeRepository.Example.DataModel;

namespace MongoTypeRepository.Example
{
    public class BookExample
    {
        private BooksRepository _bookRepo;

        public BookExample()
        {
            _bookRepo = new BooksRepository();
        }

        public void CreateBooks()
        {
            Book book = CreateBook("1", "Strongly typed repository");
            _bookRepo.Save(book);

            Book book2 = CreateBook("anyKey", "Plum fiction");
            _bookRepo.Save(book2);
        }

        public IQueryable<string> GetByLinq()
        {
            var bookResults = from book in _bookRepo.CollectionQuery
                              where book.Author == "Jiri Hernik"
                              select book.Name;

            return bookResults;
        }

        public Book GetById()
        {
            return _bookRepo.GetById("1");
        }

        public int GetByCustomRepositoryMethod()
        {
            return _bookRepo.GetBookCount();
        }

        private static Book CreateBook(string id, string name)
        {
            return new Book
            {
                Id = id,
                Author = "Jiri Hernik",
                DatePublished = new DateTime(2016, 12, 19),
                Name = name,
                Chapters = new[]
                {
                    new BookChapter
                    {
                        Name = "Introduction",
                        Content = "Lorem ipsum"
                    }
                }
            };
        }
    }
}
