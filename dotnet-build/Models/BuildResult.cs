using System;
using System.Collections.Generic;

namespace dotnet_build.Models
{
    public class BuildResult
    {
        public bool BuildSucceeded { get; internal set; }
        public string EngineVersion { get; internal set; }
        public IReadOnlyList<BuildError> Errors { get; internal set; }
        public IReadOnlyList<BuildError> Warnings { get; internal set; }
        public TimeSpan Duration { get; internal set; }
    }
}
