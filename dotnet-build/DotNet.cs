using dotnet_build.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace dotnet_build
{
    public static class DotNet
    {
        [STAThread]
        public static async Task<BuildResult> Build(FileInfo file)
        {
            if (file is null) throw new ArgumentNullException(nameof(file));
            if (!file.Exists) throw new FileNotFoundException();
            if (file.Extension != ".csproj") throw new NotSupportedException();

            var filepath = file.FullName.Contains(' ') ? $"\"{file.FullName}\"" : file.FullName;
            var buildResult = await RunBuild(filepath);
            return BuildResultParser.Parse(buildResult);
        }

        private static async Task<string> RunBuild(string filepath)
        {
            var result = new StringWriter();
            await Process
                .Start(new ProcessStartInfo("dotnet", $"build {filepath}")
                {
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                })
                .PipeOutputToStream(result)
                .WaitForExitAsync();

            return result.ToString();
        }

        private static Process PipeOutputToStream(this Process process, StringWriter result)
        {
            process.Exited += (o, e) 
                => result.WriteLine(process.StandardOutput.ReadToEnd());

            return process;
        }
    }
}
