using System;
using System.Collections.Generic;
using System.IO;
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

        //[InlineData("net461")]
        //[InlineData("net462")]
        //[Theory(Skip = "Can't make this work yet")]
        [Fact]
        public void BindingRedirectsCanBeProvided()
        {
            Func<ProjectInstance> factory = () => new ProjectInstance("BindRedirected.csproj", new Dictionary<string, string>
            {
                { "TargetFramework", "net461" },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
                { nameof(ThisAssembly.Project.Properties.MSBuildExtensionsPath), ThisAssembly.Project.Properties.MSBuildExtensionsPath },
                { nameof(ThisAssembly.Project.Properties.NuGetRestoreTargets), ThisAssembly.Project.Properties.NuGetRestoreTargets },
                { nameof(ThisAssembly.Project.Properties.CSharpCoreTargetsPath), ThisAssembly.Project.Properties.CSharpCoreTargetsPath },
                { nameof(ThisAssembly.Project.Properties.RoslynTargetsPath), ThisAssembly.Project.Properties.RoslynTargetsPath },
            }, "15.0", new ProjectCollection());

            var result = Builder.Build(factory(), "Restore").AssertSuccess();
            result = Builder.Build(factory(), "ReportBindingRedirects").AssertSuccess();
        }
    }
}