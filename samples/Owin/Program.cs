using System;
using Microsoft.Owin.Hosting;

namespace Owin
{
    public class Program
    {
        static void Main(string[] args)
        {
            const string baseAddress = "http://localhost:9000/";
            WebApp.Start(baseAddress);
            Console.WriteLine("Press ENTER to exit program...");
            Console.ReadLine();
        }
    }
}