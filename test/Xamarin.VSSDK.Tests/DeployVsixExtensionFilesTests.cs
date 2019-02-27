using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Xunit;
using Xunit.Abstractions;

namespace Xamarin.VSSDK.Tests
{
    public class DeployVsixExtensionFilesTests : VsTest
    {
        // This resolves the SDKs to the same location that was used to compile this project.
        static DeployVsixExtensionFilesTests() => Environment.SetEnvironmentVariable(
            nameof(ThisAssembly.Project.Properties.MSBuildSDKsPath),
            ThisAssembly.Project.Properties.MSBuildSDKsPath);

        public DeployVsixExtensionFilesTests(ITestOutputHelper output) : base(output) { }

        [InlineData("net461")]
        [InlineData("net462")]
        [Theory(Skip = "Can't make this work yet")]
        public void ExtensionFilesAreDeployedWhenBuidingExtension(string targetFramework)
        {
            var vsixDeploymentPath = GetVsixDeploymentPath();
            if (!string.IsNullOrEmpty(vsixDeploymentPath) && Directory.Exists(vsixDeploymentPath))
                Directory.Delete(vsixDeploymentPath, true);

            Func<ProjectInstance> factory = () => new ProjectInstance("VsixTemplate.csproj", new Dictionary<string, string>
            {
                { "TargetFramework", targetFramework },
                { "GeneratePkgDefFile", "true"},
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
                { "VSSDKTargetPlatformRegRootSuffix", RootSuffix },
                { nameof(ThisAssembly.Project.Properties.MSBuildExtensionsPath), ThisAssembly.Project.Properties.MSBuildExtensionsPath },
                { nameof(ThisAssembly.Project.Properties.NuGetRestoreTargets), ThisAssembly.Project.Properties.NuGetRestoreTargets },
                { nameof(ThisAssembly.Project.Properties.CSharpCoreTargetsPath), ThisAssembly.Project.Properties.CSharpCoreTargetsPath },
                { nameof(ThisAssembly.Project.Properties.RoslynTargetsPath), ThisAssembly.Project.Properties.RoslynTargetsPath },
            }, null, new ProjectCollection());


            var result = Builder.Build(factory(), "Restore").AssertSuccess();
            result = Builder.Build(factory(), "Build").AssertSuccess();

            vsixDeploymentPath = GetVsixDeploymentPath();

            Assert.True(Directory.Exists(vsixDeploymentPath));
            Assert.True(File.Exists(Path.Combine(vsixDeploymentPath, "VsixTemplate.dll")), "VsixTemplate.dll not found");
            Assert.True(File.Exists(Path.Combine(vsixDeploymentPath, "VsixTemplate.pkgdef")), "VsixTemplate.pkgdef not found");
            Assert.True(File.Exists(Path.Combine(vsixDeploymentPath, "extension.vsixmanifest")), "extension.vsixmanifest found");
        }
    }
}