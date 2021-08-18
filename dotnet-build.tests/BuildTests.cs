//using System;
//using System.IO;
//using System.Linq;
//using Xunit;

//namespace dotnet_build.tests
//{
//    public class BuildTests
//    {
//        private readonly DirectoryInfo _solutionDir = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.Parent;
//        private DirectoryInfo TestProjectDir => _solutionDir?.GetDirectories().FirstOrDefault(x => x.Name == "dotnet-build.buildme-project");
//        private FileInfo TestProjectFile => TestProjectDir?.GetFiles().FirstOrDefault(x => x.Name == "dotnet-build.buildme-project.csproj");

//        [Fact(DisplayName = "Should access solution directory")]
//        public void SolutionDirAccess()
//        {
//            Assert.NotNull(_solutionDir);
//        }

//        [Fact(DisplayName = "Should have the 'buildme-project' in the solution directory")]
//        public void SearchTestProject()
//        {
//            Assert.NotNull(TestProjectDir);
//            Assert.NotNull(TestProjectFile);
//        }

//        [Fact(DisplayName = "Should block invalid files")]
//        public void FilepathVerifier()
//        {
//            Assert.Throws(new ArgumentNullException().GetType(), () => DotNet.Build(null));
//            Assert.Throws(new FileNotFoundException().GetType(), () => DotNet.Build(new FileInfo("File.csproj")));
//            Assert.Throws(new NotSupportedException().GetType(), () => DotNet.Build(new FileInfo("TestFile.txt")));
//        }

//        [Fact(DisplayName = "Should run successfully")]
//        public void Start()
//        {
//            DotNet.Build(TestProjectFile);
//        }
//    }
//}
