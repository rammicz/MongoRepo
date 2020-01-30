using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoTypeRepository.Example.CustomRepositories;
using MongoTypeRepository.Example.DataModel;

namespace MongoTypeRepository.Example
{
    public class BookExample
    {
        private BooksRepository _bookRepo;
        private ObjectId _bookId;

        public BookExample()
        {
            _bookRepo = new BooksRepository();
        }

        public async Task CreateBooks()
        {
            _bookId = ObjectId.GenerateNewId();
            Book book = CreateBook(_bookId, "Strongly typed repository");
            await _bookRepo.SaveAsync(book);

            Book book2 = CreateBook(ObjectId.GenerateNewId(), "Plum fiction");
            await _bookRepo.SaveAsync(book2);
        }

        public async Task CreateBooks(int booksToCreate)
        {
            List<Book> books = new List<Book>(booksToCreate);

            for (int counter = 0; counter++ < booksToCreate;)
            {
                books.Add(CreateBook(ObjectId.GenerateNewId(), "Plum fiction " + counter));
            }

            await _bookRepo.SaveAsync(books);
        }

        public IQueryable<string> GetByLinq()
        {
            var bookResults = from book in _bookRepo.CollectionQuery
                where book.Author == "Jiri Hernik"
                select book.Name;

            return bookResults;
        }

        public async Task<Book> GetById()
        {
            return await _bookRepo.GetByIdAsync(_bookId);
        }

        public async Task<int> GetByCustomRepositoryMethod()
        {
            return await _bookRepo.GetBookCountAsync();
        }

        private static Book CreateBook(ObjectId id, string name)
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
