using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace MongoTypeRepository.Example
{
    internal class Program
    {

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Creating repository book");
            var bookExample = new BookExample();
            Console.WriteLine();

            Console.WriteLine("Saving books to DB");
            await bookExample.CreateBooks();
            Console.WriteLine();

            Console.WriteLine("Getting item from DB by ID");
            Console.WriteLine((await bookExample.GetById()).ToJson());
            Console.WriteLine();

            Console.WriteLine("Getting item names from DB by Linq");
            bookExample.GetByLinq().ToList().ForEach(Console.WriteLine);
            Console.WriteLine();

            Console.WriteLine("Getting item from DB by custom repository method");
            Console.WriteLine(await bookExample.GetByCustomRepositoryMethod());
            Console.WriteLine();

            // Do not forget, to check how wrapped objects works
            Console.WriteLine("Aliens are coming now");
            var alienShipExample = new AlienShipExample();
            Console.WriteLine();

            Console.WriteLine("Saving AlienShips to DB");
            await alienShipExample.CreateShip();
            Console.WriteLine();

            Console.WriteLine("Getting item from DB by custom repository method");
            Console.WriteLine(alienShipExample.GetByCustomRepositoryMethod().ToJson());
            Console.WriteLine();


            Console.WriteLine("1000 async saves");
            Stopwatch stop = new Stopwatch();
            stop.Start();
            await bookExample.CreateBooks(1000);
            stop.Stop();
            Console.WriteLine("1000 async saves done in " + stop.Elapsed);


            Console.WriteLine("press key...");
            Console.ReadKey();
        }
    }
}