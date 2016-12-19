using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoTypeRepository.Example.CustomRepositories;
using MongoTypeRepository.Example.DataModel;

namespace MongoTypeRepository.Example
{
    internal class Program
    {


        private static void Main(string[] args)
        {
            Console.WriteLine("Creating repository book");
            var bookExample = new BookExample();
            Console.WriteLine();

            Console.WriteLine("Saving books to DB");
            bookExample.CreateBooks();
            Console.WriteLine();


            Console.WriteLine("Getting item from DB by ID");
            Console.WriteLine(bookExample.GetById().ToJson());
            Console.WriteLine();

            Console.WriteLine("Getting item names from DB by Linq");
            bookExample.GetByLinq().ToList().ForEach(Console.WriteLine);
            Console.WriteLine();

            Console.WriteLine("Getting item from DB by custom repository method");
            Console.WriteLine(bookExample.GetByCustomRepositoryMethod());
            Console.WriteLine();

            // Do not forget, to check how wrapped objects works
            Console.WriteLine("Aliens are coming now");
            var alienShipExample = new AlienShipExample();
            Console.WriteLine();

            Console.WriteLine("Saving AlienShips to DB");
            alienShipExample.CreateShip();
            Console.WriteLine();

            Console.WriteLine("Getting item from DB by custom repository method");
            Console.WriteLine(alienShipExample.GetByCustomRepositoryMethod().ToJson());
            Console.WriteLine();
            Console.WriteLine("press key...");
            Console.ReadKey();

        }


    }
}