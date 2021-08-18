namespace dotnet_build.Models
{
    class BuildResult
    {
        public bool BuildSucceeded { get; internal set; }
        public string EngineVersion { get; internal set; }
    }
}
