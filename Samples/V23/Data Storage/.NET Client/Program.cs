using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SAF_Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            // write using raw client
            Console.WriteLine("Writing data using raw client...");
            RawClientExample rawClient = new RawClientExample();
            rawClient.StoreData();

            // write using helper class
            Console.WriteLine("Writing data using helper class...");
            HelperClassExample helperClass = new HelperClassExample();
            helperClass.StoreData();

            // write using helper session
            Console.WriteLine("Writing data using helper session...");
            HelperSessionExample helperSession = new HelperSessionExample();
            helperSession.StoreData();

            Console.WriteLine("Disconnecting...");

            // disconnect raw client
            Thread.Sleep(3000);
            rawClient.ReleaseSession();

            // disconnect helper class
            Thread.Sleep(3000);
            helperClass.ReleaseSession();

            // disconnect helper session
            Thread.Sleep(3000);
            helperSession.Disconnect();

            Console.WriteLine("Finished.");
            Console.ReadLine();
        }
    }
}
