using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Xamarin.VSSDK.Tests
{
    public class ValidateVsixReferencedAssembliesTests
    {
        ITestOutputHelper output;

        public ValidateVsixReferencedAssembliesTests(ITestOutputHelper output) =>
            this.output = output;

        [Fact]
        public void when_targeting_dev14_and_shell_15_is_referenced_then_fails()
        {
            var task = CreateTask();
            task.Dev = "14.0";
            task.VsixSourceItems = GetTaskItems("Foo.dll", "Foo.txt");
            task.ReferencedAssembliesToValidate = GetTaskItems("Shell");

            Assert.False(task.Execute());
        }

        [Fact]
        public void when_targeting_dev15_and_shell_15_is_referenced_then_succeeds()
        {
            var task = CreateTask();
            task.Dev = "15.0";
            task.VsixSourceItems = GetTaskItems("Foo.dll", "Foo.txt");
            task.ReferencedAssembliesToValidate = GetTaskItems("Shell");

            Assert.True(task.Execute());
        }

        [Fact]
        public void when_targeting_dev14_and_shell_is_not_validated_then_succeeds()
        {
            var task = CreateTask();
            task.Dev = "14.0";
            task.VsixSourceItems = GetTaskItems("Foo.dll", "Foo.txt");
            // FooRef is not included to be validated
            task.ReferencedAssembliesToValidate = GetTaskItems("Editor");

            Assert.True(task.Execute());
        }

        [Fact]
        public void when_targeting_dev14_and_shell_15_is_explicitly_excluded_then_succeeds()
        {
            var task = CreateTask();
            task.Dev = "14.0";
            task.VsixSourceItems = GetTaskItems("Foo.dll", "Foo.txt");
            task.ReferencedAssembliesToValidate = GetTaskItems("Shell");
            task.ExcludeValidateReferencedAssemblies = GetTaskItems("Shell.15");

            Assert.True(task.Execute());
        }

        ITaskItem[] GetTaskItems(params string[] values) =>
            values.Select(x => new TaskItem(x)).ToArray();

        ValidateVsixReferencedAssemblies CreateTask() =>
            new ValidateVsixReferencedAssemblies(x =>
            {
                if (x == "Foo.dll")
                    return new AssemblyName[]
                    {
                        new AssemblyName("Shell.14.dll") { Version = new Version("14.0") },
                        new AssemblyName("Shell.15.dll") { Version = new Version("15.0") }
                    };

                return Enumerable.Empty<AssemblyName>();
            })
            {
                BuildEngine = new MockBuildEngine(output)
            };
    }
}