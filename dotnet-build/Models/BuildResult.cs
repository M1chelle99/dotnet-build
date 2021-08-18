using System.Collections.Generic;

namespace dotnet_build.Models
{
    class BuildResult
    {
        public bool BuildSucceeded { get; internal set; }
        public string EngineVersion { get; internal set; }
        public IReadOnlyList<Error> Errors { get; internal set; }
        public IReadOnlyList<Error> Warnings { get; internal set; }
    }
}
