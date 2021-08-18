using dotnet_build.Models;
using System;
using System.Linq;

namespace dotnet_build
{
    static class BuildResultParser
    {
        public static BuildResult Parse(string resultLog)
        {
            var result = new BuildResult();
            var lines = resultLog.Split(Environment.NewLine).TrimAll();
            result.EngineVersion = GetEngineVersion(lines);
            result.BuildSucceeded = GetBuildStatus(lines);


            return result;
        }

        public static string GetEngineVersion(string[] lines)
        {
            if (!lines[0].TryFind("Build Engine version ", true, out var _, out var start))
                throw new ParserException("Cannot parse engine version.");
            if (!lines[0].TryFind(" for", true, out var end, out var _))
                throw new ParserException("Cannot parse engine version.");

            return lines[0][start..end];
        }

        public static bool GetBuildStatus(string[] lines)
        {
            var line = lines.FirstOrDefault(x => x.StartsWith("Build "));
            if (line == null) throw new ParserException("Cannot parse build status.");

            if (!line.TryFind("Build ", true, out var _, out var end))
                throw new ParserException("Cannot parse build status.");
            if (!line.TryFind(".", false, out var start, out var _)) 
                throw new ParserException("Cannot parse build status.");

            return line[end..start].ToLower() switch
            {
                "succeeded" => true,
                "failed" => false,
                _ => throw new ParserException("Cannot parse build status.")
            };
        }

        public static bool TryFind(this string source, string str, bool startLeft, out int start, out int end)
        {
            start = startLeft ? source.IndexOf(str) : source.LastIndexOf(str);
            end = start + str.Length;

            if (start == -1)
                return false;
            else
                return true;
        }

        private static string[] TrimAll(this string[] strings)
        {
            for (int i = 0; i < strings.Length; i++)
                strings[i] = strings[i].Trim();

            return strings;
        }
    }
}

/*
 
Microsoft (R) Build Engine version 16.11.0+0538acc04 for .NET
Copyright (C) Microsoft Corporation. All rights reserved.

  Determining projects to restore...
  All projects are up-to-date for restore.
F:\Repositories\dotnet-build\dotnet-build.buildme-project\Program.cs(12,61): error CS1525: Invalid expression term '}' [F:\Repositories\dotnet-build\dotnet-build.buildme-project\dotnet-build.buildme-project.csproj]
F:\Repositories\dotnet-build\dotnet-build.buildme-project\Program.cs(12,61): error CS1002: ; expected [F:\Repositories\dotnet-build\dotnet-build.buildme-project\dotnet-build.buildme-project.csproj]

Build FAILED.

F:\Repositories\dotnet-build\dotnet-build.buildme-project\Program.cs(12,61): error CS1525: Invalid expression term '}' [F:\Repositories\dotnet-build\dotnet-build.buildme-project\dotnet-build.buildme-project.csproj]
F:\Repositories\dotnet-build\dotnet-build.buildme-project\Program.cs(12,61): error CS1002: ; expected [F:\Repositories\dotnet-build\dotnet-build.buildme-project\dotnet-build.buildme-project.csproj]
    0 Warning(s)
    2 Error(s)

Time Elapsed 00:00:01.08

*/

/*

Microsoft (R) Build Engine version 16.11.0+0538acc04 for .NET
Copyright (C) Microsoft Corporation. All rights reserved.

  Determining projects to restore...
  All projects are up-to-date for restore.
  dotnet-build.buildme-project -> F:\Repositories\dotnet-build\dotnet-build.buildme-project\bin\Debug\net5.0\dotnet-build.buildme-project.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:01.98

*/