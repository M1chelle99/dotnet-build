using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_build.starter
{
    class Program
    {
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
                .First(x => x.Name == "dotnet-build.buildme-project.csproj");
    }
}
