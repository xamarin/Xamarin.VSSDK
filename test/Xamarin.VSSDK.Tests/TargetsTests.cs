using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Xunit;
using Xunit.Abstractions;
using static Builder;

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
            var project = new ProjectInstance("Template.csproj", Global(new Dictionary<string, string>
            {
                { "TargetFrameworks", "net45;net46;net472" },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
            }), null, new ProjectCollection(Global()));

            Builder.Build(project, "Restore");
            var result = Builder.Build(project, "Build");

            Assert.Equal(BuildResultCode.Failure, result.BuildResult.OverallResult);
            if (result.Logger.Errors[0].Code != "XVSSDK0001")
                Process.Start(result.StructuredLogger.Parameters);

            Assert.Equal("XVSSDK0001", result.Logger.Errors[0].Code);
        }

        [Fact]
        public void CrossTargetingBuildSucceedsWhenBuildingInsideVisualStudio()
        {
            var project = new ProjectInstance("Template.csproj", Global(new Dictionary<string, string>
            {
                { "TargetFrameworks", "net45;net46;net472" },
                { "BuildingInsideVisualStudio", "true" },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
            }), null, new ProjectCollection(Global()));

            Build(project, "Restore").AssertSuccess();
            Build(project, "Build").AssertSuccess();
        }

        [Fact]
        public void TargetFrameworkIsSetFromActiveDebugFrameworkWhenCrossTargetingWhenBuildingInsideVisualStudio()
        {
            var project = new Project("Template.csproj", new Dictionary<string, string>
            {
                { "TargetFrameworks", "net45;net46;net472" },
                { "ActiveDebugFramework", "net46" },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
                { "BuildingInsideVisualStudio", "true" }
            }, null, new ProjectCollection());

            Assert.Equal("net46", project.GetPropertyValue("TargetFramework"));
        }

        [Fact]
        public void TargetFrameworkIsNotSetFromActiveDebugFrameworkWhenCrossTargetingWhenNotBuildingInsideVisualStudio()
        {
            var project = new Project("Template.csproj", new Dictionary<string, string>
            {
                { "TargetFrameworks", "net45;net46;net472" },
                { "ActiveDebugFramework", "net46" },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
            }, null, new ProjectCollection());

            Assert.NotEqual("net46", project.GetPropertyValue("TargetFramework"));
        }

        [Fact]
        public void TargetFrameworksIsSetFromDevWhenCrossTargeting()
        {
            var project = new Project("Template.csproj", new Dictionary<string, string>
            {
                { "TargetFrameworks", "net45;net46;net472" },
                { "Dev", "15.0" },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
            }, null, new ProjectCollection());

            Assert.Equal("net46", project.GetPropertyValue("TargetFrameworks"));
        }

        [Fact]
        public void DevOverridesActiveDebugFrameworkWhenCrossTargeting()
        {
            var project = new Project("Template.csproj", new Dictionary<string, string>
            {
                { "TargetFrameworks", "net45;net46;net472" },
                { "ActiveDebugFramework", "net45" },
                { "Dev", "15.0" },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
                { "BuildingInsideVisualStudio", "true" },
            }, null, new ProjectCollection());

            Assert.Equal("net46", project.GetPropertyValue("TargetFramework"));
        }

        [Fact]
        public void DevOverridesTargetFrameworksWhenCrossTargeting()
        {
            var project = new Project("Template.csproj", new Dictionary<string, string>
            {
                { "TargetFrameworks", "net45;net46;net472" },
                { "Dev", "15.0" },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
            }, null, new ProjectCollection());

            Assert.Equal("net46", project.GetPropertyValue("TargetFrameworks"));
        }

        [Fact]
        public void DevIsSetByTargetFrameworkWhenCrossTargeting()
        {
            var project = new Project("Template.csproj", new Dictionary<string, string>
            {
                { "TargetFrameworks", "net45;net46;net472" },
                { "TargetFramework", "net46" },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
            }, null, new ProjectCollection());

            Assert.Equal("15.0", project.GetPropertyValue("Dev"));
        }
    }
}
