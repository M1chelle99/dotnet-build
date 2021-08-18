namespace dotnet_build.Models
{
    class Error
    {
        public string Code { get; internal set; }
        public string Description { get; internal set; }
        public string FilePath { get; internal set; }
        public string RelativePath { get; internal set; }
        public int Line { get; internal set; }
        public int Char { get; internal set; }
    }
}
