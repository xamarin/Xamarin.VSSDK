using System.Collections.Generic;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Xunit;
using Xunit.Abstractions;

namespace Xamarin.VsSDK.Tests
{
    public class TargetsTests
    {
        ITestOutputHelper output;

        public TargetsTests(ITestOutputHelper output) => this.output = output;

        [Fact]
        public void CrossTargetingBuildFailsWhenNotBuildingInsideVisualStudio()
        {
            var project = new ProjectInstance("Template.csproj", new Dictionary<string, string>
            {
                { "TargetFrameworks", "net46;net461;net462" },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
            }, "15.0", new ProjectCollection());

            var result = Builder.Build(project, "Build");

            Assert.Equal(BuildResultCode.Failure, result.BuildResult.OverallResult);
            Assert.Equal("XVsSDK0001", result.Logger.Errors[0].Code);
        }

        [Fact]
        public void CrossTargetingBuildSucceedsWhenBuildingInsideVisualStudio()
        {
            var project = new ProjectInstance("Template.csproj", new Dictionary<string, string>
            {
                { "TargetFrameworks", "net46;net461;net462" },
                { "BuildingInsideVisualStudio", "true" },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
            }, "15.0", new ProjectCollection());

            var result = Builder.Build(project, "Build");

            Assert.Equal(BuildResultCode.Success, result.BuildResult.OverallResult);
            Assert.Equal(TargetResultCode.Success, result.ResultCode);
        }

        [Fact]
        public void TargetFrameworkIsSetFromActiveDebugFrameworkWhenCrossTargeting()
        {
            var project = new Project("Template.csproj", new Dictionary<string, string>
            {
                { "TargetFrameworks", "net46;net461;net462" },
                { "ActiveDebugFramework", "net461" },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
            }, "15.0", new ProjectCollection());

            Assert.Equal("net461", project.GetPropertyValue("TargetFramework"));
        }

        [Fact]
        public void TargetFrameworkIsSetFromDevWhenCrossTargeting()
        {
            var project = new Project("Template.csproj", new Dictionary<string, string>
            {
                { "TargetFrameworks", "net46;net461;net462" },
                { "Dev", "15.0" },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
            }, "15.0", new ProjectCollection());

            Assert.Equal("net462", project.GetPropertyValue("TargetFramework"));
        }

        [Fact]
        public void DevOverridesActiveDebugFrameworkWhenCrossTargeting()
        {
            var project = new Project("Template.csproj", new Dictionary<string, string>
            {
                { "TargetFrameworks", "net46;net461;net462" },
                { "ActiveDebugFramework", "net461" },
                { "Dev", "15.0" },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
            }, "15.0", new ProjectCollection());

            Assert.Equal("net462", project.GetPropertyValue("TargetFramework"));
        }

        [Fact]
        public void DevOverridesTargetFrameworksWhenCrossTargeting()
        {
            var project = new Project("Template.csproj", new Dictionary<string, string>
            {
                { "TargetFrameworks", "net46;net461;net462" },
                { "Dev", "15.0" },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
            }, "15.0", new ProjectCollection());

            Assert.Equal("net462", project.GetPropertyValue("TargetFrameworks"));
        }

        [Fact]
        public void DevIsSetByTargetFrameworkWhenCrossTargeting()
        {
            var project = new Project("Template.csproj", new Dictionary<string, string>
            {
                { "TargetFrameworks", "net46;net461;net462" },
                { "TargetFramework", "net461" },
                { "Configuration", ThisAssembly.Project.Properties.Configuration },
            }, "15.0", new ProjectCollection());

            Assert.Equal("14.0", project.GetPropertyValue("Dev"));
        }
    }
}
