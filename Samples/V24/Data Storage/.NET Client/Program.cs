using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SAF_Examples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // write using raw client
            Console.WriteLine("Writing data using raw client...");
            RawClientExample rawClient = new();
            await rawClient.StoreDataAsync();

            // write using extension methods
            Console.WriteLine("Writing data using extension methods...");
            ExtensionMethodsExample extensionMethodsClient = new();
            await extensionMethodsClient.StoreDataAsync();

            Console.WriteLine("Disconnecting...");

            // disconnect raw client
            Thread.Sleep(3000);
            await rawClient.DisconnectAsync();

            // disconnect extension methods client
            Thread.Sleep(3000);
            await extensionMethodsClient.DisconnectAsync();

            Console.WriteLine("Finished.");
            Console.ReadLine();
        }
    }
}
