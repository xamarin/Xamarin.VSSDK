using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging.StructuredLogger;
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

    public static TargetResult Build(ProjectInstance project, string targets, 
        Dictionary<string, string> properties = null, 
        ITestOutputHelper output = null, LoggerVerbosity? verbosity = null, 
        [CallerMemberName] string caller = "")
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
            var structured = new StructuredLogger { Verbosity = verbosity.GetValueOrDefault(), Parameters = caller + ".binlog" };
            parameters.Loggers = new ILogger[] { logger, structured };

            var result = manager.Build(parameters, request);
            if (verbosity >= LoggerVerbosity.Detailed)
                output.WriteLine($"msbuild {project.ProjectFileLocation.File} /t:{targets.Replace(';', ',')} " +
                    string.Join(" ", properties.Select(kv => $"/p:{kv.Key}=\"{kv.Value}\"")) + " " + 
                    string.Join(" ", project.GlobalProperties.Select(kv => $"/p:{kv.Key}=\"{kv.Value}\"")));

            return new TargetResult(result, targets, logger, structured);
        }
    }

    public class TargetResult : ITargetResult
    {
        public TargetResult(BuildResult result, string target, TestOutputLogger logger, StructuredLogger structured)
        {
            BuildResult = result;
            Target = target;
            Logger = logger;
            StructuredLogger = structured;
        }

        public BuildResult BuildResult { get; }

        public StructuredLogger StructuredLogger { get; }

        public TestOutputLogger Logger { get; }

        public string Target { get; }

        public Exception Exception => BuildResult[Target].Exception;

        public ITaskItem[] Items => BuildResult[Target].Items;

        public TargetResultCode ResultCode => BuildResult[Target].ResultCode;

        public TargetResult AssertSuccess()
        {
            if (!BuildResult.ResultsByTarget.ContainsKey(Target))
            {
                Logger.Output?.WriteLine(ToString());
                Assert.False(true, "Build results do not contain output for target " + Target);
            }

            if (ResultCode != TargetResultCode.Success)
            {
                Logger.Output?.WriteLine(ToString());
#if DEBUG
                Process.Start(StructuredLogger.Parameters);
#endif
            }

            Assert.Equal(TargetResultCode.Success, ResultCode);
            return this;
        }

        public override string ToString()
            => string.Join(Environment.NewLine, Logger.Warnings
                .Select(e => e.Message)
                .Concat(Logger.Errors.Select(e => e.Message)));
    }
}