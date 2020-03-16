using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Xunit;
using Xunit.Abstractions;

namespace Xamarin.VSSDK.Tests
{
    public class GenerateBindingRedirectsTests : VsTest
    {
        // This resolves the SDKs to the same location that was used to compile this project.
        static GenerateBindingRedirectsTests() => Environment.SetEnvironmentVariable(
            nameof(ThisAssembly.Project.Properties.MSBuildSDKsPath),
            ThisAssembly.Project.Properties.MSBuildSDKsPath);

        public GenerateBindingRedirectsTests(ITestOutputHelper output) : base(output) { }

        [InlineData("net45")]
        [InlineData("net46")]
        [Theory]
        public void BindingRedirectsCanBeProvided(string targetFramework)
        {
            Func<ProjectInstance> factory = () => new ProjectInstance("BindRedirected.csproj", new Dictionary<string, string>
            {
                { "TargetFramework", targetFramework },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
                { nameof(ThisAssembly.Project.Properties.MSBuildExtensionsPath), ThisAssembly.Project.Properties.MSBuildExtensionsPath },
                { nameof(ThisAssembly.Project.Properties.NuGetRestoreTargets), ThisAssembly.Project.Properties.NuGetRestoreTargets },
                { nameof(ThisAssembly.Project.Properties.CSharpCoreTargetsPath), ThisAssembly.Project.Properties.CSharpCoreTargetsPath },
                { nameof(ThisAssembly.Project.Properties.RoslynTargetsPath), ThisAssembly.Project.Properties.RoslynTargetsPath },
            }, null, new ProjectCollection());

            var result = Builder.Build(factory(), "Restore").AssertSuccess();
            result = Builder.Build(factory(), "ReportBindingRedirects").AssertSuccess();
        }

        [Fact]
        public void BindingRedirectsCanExclude()
        {
            Func<ProjectInstance> factory = () => new ProjectInstance("BindRedirected.csproj", new Dictionary<string, string>
            {
                { "TargetFramework", "netstandard2.0" },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
                { nameof(ThisAssembly.Project.Properties.MSBuildExtensionsPath), ThisAssembly.Project.Properties.MSBuildExtensionsPath },
                { nameof(ThisAssembly.Project.Properties.NuGetRestoreTargets), ThisAssembly.Project.Properties.NuGetRestoreTargets },
                { nameof(ThisAssembly.Project.Properties.CSharpCoreTargetsPath), ThisAssembly.Project.Properties.CSharpCoreTargetsPath },
                { nameof(ThisAssembly.Project.Properties.RoslynTargetsPath), ThisAssembly.Project.Properties.RoslynTargetsPath },
            }, null, new ProjectCollection());

            var result = Builder.Build(factory(), "Restore").AssertSuccess();
            result = Builder.Build(factory(), "ReportBindingRedirects").AssertSuccess();

            Assert.Empty(result.Items.Where(x => x.GetMetadata("PackageName") == "System.Reactive.Linq"));
            Assert.NotEmpty(result.Items.Where(x => x.GetMetadata("PackageName") == "System.Reactive"));
            Assert.NotEmpty(result.Items.Where(x => x.GetMetadata("PackageName") == "System.Reactive.Core"));
        }
    }
}