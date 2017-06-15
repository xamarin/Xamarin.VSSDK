using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Xamarin.SampleVsix.Properties;

namespace Xamarin.SampleVsix
{
    [Guid("BC1550C5-64C9-4B9A-A4F6-7D477C9178F1")]
    [InstalledProductRegistration("Sample", "Sample", "1.0 (asdf)")]
    public class SamplePackage : Package
    {
        //public override string ToString()
        //{
        //    return Strings.Foo;
        //}
    }
}
