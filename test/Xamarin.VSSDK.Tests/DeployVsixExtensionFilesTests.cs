using System.Collections.Generic;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Xunit;
using Xunit.Abstractions;

namespace Xamarin.VSSDK.Tests
{
    public class DeployVsixExtensionFilesTests
    {
        ITestOutputHelper output;

        public DeployVsixExtensionFilesTests(ITestOutputHelper output) => this.output = output;

        [Fact]
        public void ExtensionFilesAreDeployedWhenBuidingExtension ()
        {
            var project = new ProjectInstance("VsixTemplate.csproj", new Dictionary<string, string>
            {
                { "TargetFramework", "net462" },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
            }, "15.0", new ProjectCollection());

            var result = Builder.Build(project, "Build", output: output);

            Assert.Equal(BuildResultCode.Success, result.BuildResult.OverallResult);
            Assert.Equal(TargetResultCode.Success, result.ResultCode);
        }
    }
}
