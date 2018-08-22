internal static class ModuleInitializer
{
    internal static void Run()
    {
        Microsoft.Build.Locator.MSBuildLocator.RegisterMSBuildPath(ThisAssembly.Project.Properties.MSBuildBinPath);
    }
}