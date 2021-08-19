using dotnet_build.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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
            var buildErrors = GetBuildErrors(lines);
            result.Errors = buildErrors.Where(x => x.Severity == Severity.Error).ToList();
            result.Warnings = buildErrors.Where(x => x.Severity == Severity.Warning).ToList();
            result.Duration = GetBuildDuration(lines);

            return result;
        }

        static TimeSpan GetBuildDuration(string[] lines)
        {
            var line = lines.RemoveEmptyLines().LastOrDefault() 
                ?? throw new ParserException("Cannot parse build duration.");

            if (!line.TryFind(new Regex("\\d{2}:\\d{2}:\\d{2}.\\d{2,}"), out var start, out var _))
                throw new ParserException("Cannot parse build duration.");
            
            var time = line[start..];
            var matches = new Regex("\\d{2,}", RegexOptions.Singleline).Matches(time);
            var hours = int.Parse(matches.ElementAt(0).Value);
            var minutes = int.Parse(matches.ElementAt(1).Value);
            var seconds = int.Parse(matches.ElementAt(2).Value);
            var milliseconds = int.Parse(matches.ElementAt(3).Value) * 10; // .10 = 100 milliseconds -> *10

            try
            {
                return new TimeSpan(0, hours, minutes, seconds, milliseconds);
            } 
            catch (Exception ex)
            {
                throw new ParserException("Cannot parse build duration.", ex);
            }
        }

        static IReadOnlyList<BuildError> GetBuildErrors(string[] lines)
        {
            var errors = new List<BuildError>();

            var buildLine = lines.First(x => x.StartsWith("Build "));
            var buildLineIndex = lines.IndexOf(buildLine);

            var warningInfoLine = GetWarningInfoLine(lines);
            var warningInfoLineIndex = lines.IndexOf(warningInfoLine);

            var start = buildLineIndex + 1;
            var end = warningInfoLineIndex;

            var messages = lines[start..end].RemoveEmptyLines();
            foreach (var message in messages)
                errors.Add(ParseError(message));

            return errors;
        }

        static string GetErrorInfoLine(string[] lines)
        {
            var errorsRegex = new Regex("(\\d+) Error\\(s\\)");
            if (!TryFindLine(lines, errorsRegex, out var index))
                throw new ParserException("Cannot parse error info line.");

            return lines[index];
        }

        static string GetWarningInfoLine(string[] lines)
        {
            var warningsRegex = new Regex("(\\d+) Warning\\(s\\)");
            if (!TryFindLine(lines, warningsRegex, out var index))
                throw new ParserException("Cannot parse warning info line.");

            return lines[index];
        }

        static int GetErrorCount(string errorInfoLine)
        {
            var numberRegex = new Regex("\\d\\w");
            var errorCountStr = numberRegex.Match(errorInfoLine).Value;
            return int.Parse(errorCountStr);
        }

        static string GetEngineVersion(string[] lines)
        {
            if (!lines[0].TryFind("Build Engine version ", true, out var _, out var start))
                throw new ParserException("Cannot parse engine version.");
            if (!lines[0].TryFind(" for", true, out var end, out var _))
                throw new ParserException("Cannot parse engine version.");

            return lines[0][start..end];
        }

        static bool GetBuildStatus(string[] lines)
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

        static bool TryFind(this string source, string str, bool startLeft, out int start, out int end)
        {
            start = startLeft ? source.IndexOf(str) : source.LastIndexOf(str);
            end = start + str.Length;

            if (start == -1)
                return false;
            else
                return true;
        }

        static bool TryFind(this string source, Regex regex, out int start, out int end)
        {
            start = -1;
            end = -1;

            var result = regex.Match(source);
            if (result.Success == false) 
                return false;

            start = source.IndexOf(result.Value);
            end = start + result.Value.Length;

            return true;
        }

        static bool TryFindLine(this string[] lines, Regex regex, out int index)
        {
            for (int i = 0; i < lines.Length; i++)
                if (regex.IsMatch(lines[i]))
                {
                    index = i;
                    return true;
                }

            index = -1;
            return false;
        }

        static string[] TrimAll(this string[] strings)
        {
            for (int i = 0; i < strings.Length; i++)
                strings[i] = strings[i].Trim();

            return strings;
        }

        static int IndexOf(this string[] strings, string str)
        {
            for (int i = 0; i < strings.Length; i++)
                if (strings[i] == str)
                    return i;

            return -1;
        }

        static string[] RemoveEmptyLines(this string[] lines)
            => lines
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

        static BuildError ParseError(string line)
        { // F:\Repositories\dotnet-build\dotnet-build.buildme-project\Program - Copy.cs(17,17): warning CS0184: The given expression is never of the provided ('string') type [F:\Repositories\dotnet-build\dotnet-build.buildme-project\dotnet-build.buildme-project.csproj]
            var error = new BuildError();

            var parts = line.Split(": ");
            if (parts.Length < 3)
                throw new ParserException("Cannot parse the error.");

            ParseErrorLocation(parts[0], error);
            ParseErrorType(parts[1], error);
            ParseErrorDescription(parts[2], error);
            ParseErrorProject(parts[2], error);
            InsertRelativePath(error);

            return error;
        }

        static void InsertRelativePath(BuildError error)
        {
            var projectFile = new FileInfo(error.ProjectPath);
            if (!projectFile.Exists)
                throw new ParserException("Cannot parse a valid project path.");

            error.RelativePath = Path.GetRelativePath(projectFile.Directory.FullName, error.Filepath);
        }

        static void ParseErrorDescription(string line, BuildError error)
        {
            if (!line.TryFind(new Regex(" \\[.+\\]"), out var start, out var _))
                throw new ParserException("Cannot parse the errors project.");

            error.Description = line[..start];
        }

        static void ParseErrorProject(string line, BuildError error)
        {
            if (!line.TryFind(new Regex("\\[.+\\]"), out var start, out var _))
                throw new ParserException("Cannot parse the errors project.");

            error.ProjectPath = line[start..]
                .Replace("[", "")
                .Replace("]", "");
        }

        static void ParseErrorType(string line, BuildError error)
        {
            ParseErrorSeverity(line, error);
            ParseErrorCode(line, error);
        }

        static void ParseErrorLocation(string line, BuildError error)
        {
            ParseErrorFilepath(line, error);
            ParseErrorPosition(line, error);
        }

        static void ParseErrorSeverity(string line, BuildError error)
        {
            if (!line.TryFind(new Regex("warning|error", RegexOptions.IgnoreCase), out var _, out var end))
                throw new ParserException("Cannot parse the error severity.");

            error.Severity = Enum.Parse<Severity>(line[..end], true);
        }

        static void ParseErrorCode(string line, BuildError error)
        {
            error.Code = line.Split(" ").Last();
        }

        static void ParseErrorFilepath(string line, BuildError error)
        {
            if (!line.TryFind(new Regex("\\(\\d+,\\d+\\)"), out var errPositionStart, out var _))
                throw new ParserException("Cannot parse the error position.");

            error.Filepath = line[..errPositionStart];
        }

        static void ParseErrorPosition(string line, BuildError error)
        {
            if (!line.TryFind(new Regex("\\(\\d+,\\d+\\)"), out var errPositionStart, out var _))
                throw new ParserException("Cannot parse the error position.");

            var errPosition = new Regex("\\d+").Matches(line[errPositionStart..]);
            var lineIndex = errPosition.ElementAt(0).Value;
            var charIndex = errPosition.ElementAt(1).Value;

            error.Line = int.Parse(lineIndex);
            error.Char = int.Parse(charIndex);
        }
    }
}
