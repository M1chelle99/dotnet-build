using System;

namespace dotnet_build.buildme_project
{
    /// text
    class Program
    {
        // /// text
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            if (args.Length > 0)
                Console.WriteLine(string.Join(", ", args));

            int i = 0;
            if (i is string)   // CS0184  
                i++; 
            
        }
    }
}
