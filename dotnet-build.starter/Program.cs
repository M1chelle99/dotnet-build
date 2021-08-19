using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_build.starter
{
    class Program
    {
#if RELEASE
        static void Main(string[] args)
        {
            var projects = GetProjects();
            if (projects.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No projects here.. huh?");
                return;
            }

            var tasks = new List<Task>();
            foreach (var project in projects)
                tasks.Add(Task.Run(async () =>
                    await DotNet.Build(project)
                ));

            Task.WaitAll(tasks.ToArray());

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Ran all builds.");
            Console.ResetColor();
        }

        static FileInfo[] GetProjects()
            => new DirectoryInfo(Environment.CurrentDirectory)
                .GetFiles()
                .Where(x => Path.GetExtension(x.FullName) == ".csproj")
                .ToArray();
#endif
#if DEBUG
        static async Task Main()
        {
            var csProj = GetProject();
            await DotNet.Build(csProj);
        }

        static FileInfo GetProject()
            => Directory
                .GetParent(Environment.CurrentDirectory)
                .Parent
                .Parent
                .Parent
                .GetDirectories()
                .First(x => x.Name == "dotnet-build.buildme-project")
                .GetFiles()
                .First(x => x.Name == "dotnet-build.buildme project.csproj");
#endif
    }
}
