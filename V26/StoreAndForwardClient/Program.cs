using System;
using System.Threading;
using System.Threading.Tasks;

namespace StoreAndForwardClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            RawClientExample rawClient = new();
            ExtensionMethodsExample extensionMethodsClient = new();

            try
            {
                // write using raw client
                Console.WriteLine("Writing data using raw client...");
                await rawClient.StoreDataAsync();

                // write using extension methods
                Console.WriteLine("Writing data using extension methods...");
                await extensionMethodsClient.StoreDataAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(GetError(ex));
            }
            finally
            {
                Console.WriteLine("Disconnecting...");

                // disconnect raw client
                Thread.Sleep(3000);
                await rawClient.DisconnectAsync();

                // disconnect extension methods client
                Thread.Sleep(3000);
                await extensionMethodsClient.DisconnectAsync();
            }

            Console.WriteLine("Finished.");
            Console.ReadLine();
        }

        private static string GetError(Exception ex)
        {
            string error = $"Exception: {ex.Message}";
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                error += $"\nInner Exception: {ex.Message}";
            }
            return error;
        }
    }
}
