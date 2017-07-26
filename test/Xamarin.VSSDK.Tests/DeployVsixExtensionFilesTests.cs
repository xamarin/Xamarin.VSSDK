using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Xamarin.VSSDK.Tests
{
    public class DeployVsixExtensionFilesTests
    {
#if Dev14
        const string RootSuffix = "XamarinVSSDK_Tests14.0";
#elif Dev15
        const string RootSuffix = "XamarinVSSDK_Tests15.0";
#elif Dev16
        const string RootSuffix = "XamarinVSSDK_Tests16.0";
#elif Dev17
        const string RootSuffix = "XamarinVSSDK_Tests17.0";
#endif

        ITestOutputHelper output;

        public DeployVsixExtensionFilesTests(ITestOutputHelper output)
        {
            this.output = output;

#if Dev15
            Assembly
                .LoadFrom("Microsoft.VisualStudio.Sdk.BuildTasks.15.0.dll")
                .GetType("Microsoft.VisualStudio.Sdk.BuildTasks.FolderLocator")
                .GetProperty("InstanceInstallationPath", BindingFlags.Static | BindingFlags.Public)
                .SetValue(null, Environment.GetEnvironmentVariable("VSINSTALLDIR"));
#endif
        }


        string GetTargetInstancePath() =>
            Directory
                .EnumerateDirectories(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "VisualStudio"))
                .FirstOrDefault(x => x.Contains(RootSuffix));

        string GetVsixDeploymentPath()
        {
            var targetInstancePath = GetTargetInstancePath();
            if (!string.IsNullOrEmpty(targetInstancePath))
                return Path.Combine(targetInstancePath, "Extensions", "Xamarin", "Xamarin.VSSDK.Test", "1.0.0");

            return null;
        }

        [Fact]
        public void ExtensionFilesAreDeployedWhenBuidingExtension()
        {
            var vsixDeploymentPath = GetVsixDeploymentPath();
            if (!string.IsNullOrEmpty(vsixDeploymentPath) && Directory.Exists(vsixDeploymentPath))
                Directory.Delete(vsixDeploymentPath, true);

#if Dev14
            var targetFramework = "net461";
#else
            var targetFramework = "net462";
#endif

            var project = new ProjectInstance("VsixTemplate.csproj", new Dictionary<string, string>
            {
                { "TargetFramework", targetFramework },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
                { "VSSDKTargetPlatformRegRootSuffix", RootSuffix },
            }, "15.0", new ProjectCollection());

            var result = Builder.Build(project, "Build", output: output);

            Assert.Equal(BuildResultCode.Success, result.BuildResult.OverallResult);
            Assert.Equal(TargetResultCode.Success, result.ResultCode);

            vsixDeploymentPath = GetVsixDeploymentPath();

            Assert.True(Directory.Exists(vsixDeploymentPath));
            Assert.True(File.Exists(Path.Combine(vsixDeploymentPath, "VsixTemplate.dll")), "VsixTemplate.dll not found");
            Assert.True(File.Exists(Path.Combine(vsixDeploymentPath, "extension.vsixmanifest")), "extension.vsixmanifest found");
        }
    }
}