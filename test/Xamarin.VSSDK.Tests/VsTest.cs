using System;
using System.IO;
using System.Linq;
using Xunit.Abstractions;

namespace Xamarin.VSSDK.Tests
{
    public class VsTest
    {
#if Dev14
        const string BaseRootSuffix = "14.0_XVSSDK";
#elif Dev15
        const string BaseRootSuffix = "15.0_XVSSDK";
#elif Dev16
        protected const string BaseRootSuffix = "16.0_XVSSDK";
#elif Dev17
        const string BaseRootSuffix = "17.0_XVSSDK";
#endif

#if Dev14
        protected const string TargetFramework = "net461";
#else
        protected const string TargetFramework = "net462";
#endif


        ITestOutputHelper output;

        public VsTest(ITestOutputHelper output)
        {
            this.output = output;

//#if Dev14
//            Environment.SetEnvironmentVariable("VsSDKToolsPath", Path.Combine(Directory.GetCurrentDirectory(), "bin"));
//#endif

//#if Dev15
//            Assembly
//                .LoadFrom("Microsoft.VisualStudio.Sdk.BuildTasks.15.0.dll")
//                .GetType("Microsoft.VisualStudio.Sdk.BuildTasks.FolderLocator")
//                .GetProperty("InstanceInstallationPath", BindingFlags.Static | BindingFlags.Public)
//                .SetValue(null, Environment.GetEnvironmentVariable("VSINSTALLDIR"));
//#endif
        }

        protected ITestOutputHelper Output => output;

        protected string RootSuffix => BaseRootSuffix;

        string GetTargetInstancePath() =>
            Directory
                .EnumerateDirectories(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "VisualStudio"))
                .FirstOrDefault(x => x.Contains(RootSuffix));

        protected string GetVsixDeploymentPath()
        {
            var targetInstancePath = GetTargetInstancePath();
            if (!string.IsNullOrEmpty(targetInstancePath))
                return Path.Combine(targetInstancePath, "Extensions", "Xamarin", "Xamarin.VSSDK.Test", "1.0.0");

            return null;
        }
    }
}