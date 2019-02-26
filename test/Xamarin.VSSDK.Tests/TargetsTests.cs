using System;
using System.Collections.Generic;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Xunit;
using Xunit.Abstractions;

namespace Xamarin.VSSDK.Tests
{
    public class TargetsTests
    {
        // This resolves the SDKs to the same location that was used to compile this project.
        //static TargetsTests() => Environment.SetEnvironmentVariable(
        //    nameof(ThisAssembly.Project.Properties.MSBuildSDKsPath), 
        //    ThisAssembly.Project.Properties.MSBuildSDKsPath);

        ITestOutputHelper output;

        public TargetsTests(ITestOutputHelper output) => this.output = output;

        [Fact]
        public void CrossTargetingBuildFailsWhenNotBuildingInsideVisualStudio()
        {
            var project = new ProjectInstance("Template.csproj", new Dictionary<string, string>
            {
                { "TargetFrameworks", "net45;net452;net46" },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
            }, "Current", new ProjectCollection());

            Builder.Build(project, "Restore");
            var result = Builder.Build(project, "Build");

            Assert.Equal(BuildResultCode.Failure, result.BuildResult.OverallResult);
            Assert.Equal("XVSSDK0001", result.Logger.Errors[0].Code);
        }

        [Fact]
        public void CrossTargetingBuildSucceedsWhenBuildingInsideVisualStudio()
        {
            var project = new ProjectInstance("Template.csproj", new Dictionary<string, string>
            {
                { "TargetFrameworks", "net45;net452;net46" },
                { "BuildingInsideVisualStudio", "true" },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
            }, "Current", new ProjectCollection());

            Builder.Build(project, "Restore");
            var result = Builder.Build(project, "Build");

            Assert.Equal(BuildResultCode.Success, result.BuildResult.OverallResult);
            Assert.Equal(TargetResultCode.Success, result.ResultCode);
        }

        [Fact]
        public void TargetFrameworkIsSetFromActiveDebugFrameworkWhenCrossTargetingWhenBuildingInsideVisualStudio()
        {
            var project = new Project("Template.csproj", new Dictionary<string, string>
            {
                { "TargetFrameworks", "net45;net452;net46" },
                { "ActiveDebugFramework", "net452" },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
                { "BuildingInsideVisualStudio", "true" }
            }, "Current", new ProjectCollection());

            Assert.Equal("net452", project.GetPropertyValue("TargetFramework"));
        }

        [Fact]
        public void TargetFrameworkIsNotSetFromActiveDebugFrameworkWhenCrossTargetingWhenNotBuildingInsideVisualStudio()
        {
            var project = new Project("Template.csproj", new Dictionary<string, string>
            {
                { "TargetFrameworks", "net45;net452;net46" },
                { "ActiveDebugFramework", "net452" },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
            }, "Current", new ProjectCollection());

            Assert.NotEqual("net452", project.GetPropertyValue("TargetFramework"));
        }

        [Fact]
        public void TargetFrameworksIsSetFromDevWhenCrossTargeting()
        {
            var project = new Project("Template.csproj", new Dictionary<string, string>
            {
                { "TargetFrameworks", "net45;net452;net46" },
                { "Dev", "15.0" },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
            }, "Current", new ProjectCollection());

            Assert.Equal("net46", project.GetPropertyValue("TargetFrameworks"));
        }

        [Fact]
        public void DevOverridesActiveDebugFrameworkWhenCrossTargeting()
        {
            var project = new Project("Template.csproj", new Dictionary<string, string>
            {
                { "TargetFrameworks", "net45;net452;net46" },
                { "ActiveDebugFramework", "net452" },
                { "Dev", "15.0" },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
                { "BuildingInsideVisualStudio", "true" },
            }, "Current", new ProjectCollection());

            Assert.Equal("net46", project.GetPropertyValue("TargetFramework"));
        }

        [Fact]
        public void DevOverridesTargetFrameworksWhenCrossTargeting()
        {
            var project = new Project("Template.csproj", new Dictionary<string, string>
            {
                { "TargetFrameworks", "net45;net452;net46" },
                { "Dev", "15.0" },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
            }, "Current", new ProjectCollection());

            Assert.Equal("net46", project.GetPropertyValue("TargetFrameworks"));
        }

        [Fact]
        public void DevIsSetByTargetFrameworkWhenCrossTargeting()
        {
            var project = new Project("Template.csproj", new Dictionary<string, string>
            {
                { "TargetFrameworks", "net45;net452;net46" },
                { "TargetFramework", "net452" },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
            }, "Current", new ProjectCollection());

            Assert.Equal("14.0", project.GetPropertyValue("Dev"));
        }
    }
}
