using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using OmniSharp.Models;
using OmniSharp.Roslyn.CSharp.Services.Navigation;
using TestUtility;
using Xunit;
using Xunit.Abstractions;

namespace OmniSharp.Roslyn.CSharp.Tests
{
    public class GoToRegionFacts : AbstractTestFixture
    {
        public GoToRegionFacts(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public async Task CanFindRegionsInFileWithRegions()
        {
            const string source = @"
                public class Foo
                {
                      #region A
                      public string A$$Property {get; set;}
                      #endregion

                      #region B
                      public string BProperty {get; set;}
                      #endregion
                }";

            var regions = await FindRegions(source);

            Assert.Equal(4, regions.QuickFixes.Count());
            Assert.Equal("#region A", regions.QuickFixes.ElementAt(0).Text);
            Assert.Equal(3, regions.QuickFixes.ElementAt(0).Line);
            Assert.Equal("#endregion", regions.QuickFixes.ElementAt(1).Text);
            Assert.Equal(5, regions.QuickFixes.ElementAt(1).Line);
            Assert.Equal("#region B", regions.QuickFixes.ElementAt(2).Text);
            Assert.Equal(7, regions.QuickFixes.ElementAt(2).Line);
            Assert.Equal("#endregion", regions.QuickFixes.ElementAt(3).Text);
            Assert.Equal(9, regions.QuickFixes.ElementAt(3).Line);
        }

        [Fact]
        public async Task DoesNotFindRegionsInFileWithoutRegions()
        {
            const string source = @"public class Fo$$o{}";
            var regions = await FindRegions(source);
            Assert.Equal(0, regions.QuickFixes.Count());
        }

        private async Task<QuickFixResponse> FindRegions(string source)
        {
            var testFile = new TestFile("dummy.cs", source);
            var point = testFile.Content.GetPointFromPosition();

            var workspace = await CreateWorkspaceAsync(testFile);
            var controller = new GotoRegionService(workspace);

            var request = new GotoRegionRequest
            {
                Line = point.Line,
                Column = point.Offset,
                FileName = testFile.FileName,
                Buffer = testFile.Content.Code
            };

            return await controller.Handle(request);
        }
    }
}
