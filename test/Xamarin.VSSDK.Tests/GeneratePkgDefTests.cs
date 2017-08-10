using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Xunit;
using Xunit.Abstractions;

namespace Xamarin.VSSDK.Tests
{
    public class GeneratePkgDefTests : VsTest
    {
        ITestOutputHelper output;

        public GeneratePkgDefTests(ITestOutputHelper output) : base(output)
        { }

        //[Fact]
        public void PkgDefFileIsGeneratedWhenBuidingExtensionWithGeneratePkgDefFilePropertySet()
        {
            var vsixDeploymentPath = GetVsixDeploymentPath();
            if (!string.IsNullOrEmpty(vsixDeploymentPath) && Directory.Exists(vsixDeploymentPath))
                Directory.Delete(vsixDeploymentPath, true);

            var project = new ProjectInstance("VsixTemplate.csproj", new Dictionary<string, string>
            {
                { "TargetFramework", TargetFramework },
                { "GeneratePkgDefFile", "true"},
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
                { "VSSDKTargetPlatformRegRootSuffix", RootSuffix },
            }, "15.0", new ProjectCollection());

            var result = Builder.Build(project, "Restore;Rebuild", output: output);

            Assert.Equal(BuildResultCode.Success, result.BuildResult.OverallResult);

            vsixDeploymentPath = GetVsixDeploymentPath();

            Assert.True(Directory.Exists(vsixDeploymentPath));
            Assert.True(File.Exists(Path.Combine(vsixDeploymentPath, "VsixTemplate.pkgdef")), "VsixTemplate.pkgdef not found");
        }
    }
}