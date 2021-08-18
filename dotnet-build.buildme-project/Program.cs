using System;

namespace dotnet_build.buildme_project
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            if (args.Length > 0)
                Console.WriteLine(string.Join(", ", args)); -
        }
    }
}
