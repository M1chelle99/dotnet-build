namespace dotnet_build.Models
{
    public class BuildError
    {
        public Severity Severity { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Filepath { get; set; }
        public string RelativePath { get; set; }
        public string ProjectPath { get; set; }
        public int Line { get; set; }
        public int Char { get; set; }
    }

    public enum Severity
    {
        Error,
        Warning
    }
}
