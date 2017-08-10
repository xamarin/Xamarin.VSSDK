using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Xunit;
using Xunit.Abstractions;

/// <summary>
/// General-purpose MSBuild builder with support for 
/// automatic solution-configuration generation for 
/// P2P references.
/// </summary>
public static partial class Builder
{
    const string ToolsVersion = "15.0";

    public static TargetResult Build(ProjectInstance project, string targets, Dictionary<string, string> properties = null, ITestOutputHelper output = null, LoggerVerbosity? verbosity = null)
    {
        properties = properties ?? new Dictionary<string, string>();
        properties["Configuration"] = ThisAssembly.Project.Properties.Configuration;

        //if (!Debugger.IsAttached)
        // Without this, builds end up running in process and colliding with each other, 
        // especially around the current directory used to resolve relative paths in projects.
        //Environment.SetEnvironmentVariable("MSBUILDNOINPROCNODE", "0", EnvironmentVariableTarget.Process);

        using (var manager = new BuildManager(Guid.NewGuid().ToString()))
        {
            var request = new BuildRequestData(project, targets.Split(';'));
            var parameters = new BuildParameters
            {
                GlobalProperties = properties,
                DisableInProcNode = false,
                EnableNodeReuse = false,
                ShutdownInProcNodeOnBuildFinish = false,
                LogInitialPropertiesAndItems = true,
                LogTaskInputs = true,
            };

            var logger = new TestOutputLogger(output, verbosity);
            parameters.Loggers = new[] { logger };

            var result = manager.Build(parameters, request);

            return new TargetResult(result, targets, logger);
        }
    }

    public class TargetResult : ITargetResult
    {
        public TargetResult(BuildResult result, string target, TestOutputLogger logger)
        {
            BuildResult = result;
            Target = target;
            Logger = logger;
        }

        public BuildResult BuildResult { get; private set; }

        public TestOutputLogger Logger { get; private set; }

        public string Target { get; private set; }

        public Exception Exception => BuildResult[Target].Exception;

        public ITaskItem[] Items => BuildResult[Target].Items;

        public TargetResultCode ResultCode => BuildResult[Target].ResultCode;

        public void AssertSuccess(ITestOutputHelper output)
        {
            if (!BuildResult.ResultsByTarget.ContainsKey(Target))
            {
                output.WriteLine(this.ToString());
                Assert.False(true, "Build results do not contain output for target " + Target);
            }

            if (ResultCode != TargetResultCode.Success)
                output.WriteLine(this.ToString());

            Assert.Equal(TargetResultCode.Success, ResultCode);
        }

        public override string ToString()
            => string.Join(Environment.NewLine, Logger.Warnings
                .Select(e => e.Message)
                .Concat(Logger.Errors.Select(e => e.Message)));
    }
}