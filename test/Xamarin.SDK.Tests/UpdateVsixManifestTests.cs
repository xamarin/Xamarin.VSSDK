using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit;
using Xunit.Abstractions;
using System.Xml.XPath;
using System.Xml;
using System.Diagnostics;

namespace Xamarin.VsSDK.Tests
{
    public class UpdateVsixManifestTests
    {
        ITestOutputHelper output;
        XNamespace XmlNs = XNamespace.Get("http://schemas.microsoft.com/developer/vsx-schema/2011");
        XNamespace XmlNsD = XNamespace.Get("http://schemas.microsoft.com/developer/vsx-schema-design/2011");

        public UpdateVsixManifestTests(ITestOutputHelper output) => this.output = output;

        [Fact]
        public void WhenUpdatingManifestThenSucceeds()
        {
            var doc = CreateBasicManifest();

            var task = new UpdateVsixManifest(doc)
            {
                BuildEngine = new MockBuildEngine(output),
                Metadata = new TaskItem("TestId", new Dictionary<string, string>
                {
                    { "Version", "2.0" },
                    { "DisplayName", "Foo" },
                    { "Description", "Bar" },
                }),
                Installation = new TaskItem("TestId", new Dictionary<string, string>
                {
                    { "AllUsers", "true" },
                    { "Experimental", "true" },
                }),
                InstallationTargets = new ITaskItem[]
                {
                    new TaskItem("Microsoft.VisualStudio.Community", new Dictionary<string, string>
                    {
                        { "Version", "[15.0,16.0)" },
                    }),
                },
                Prerequisites = new ITaskItem[]
                {
                    new TaskItem("Microsoft.VisualStudio.Component.CoreEditor", new Dictionary<string, string>
                    {
                        { "Version", "[15.0,16.0)" },
                        { "DisplayName", "Visual Studio core editor" },
                    }),
                },
                Dependencies = new ITaskItem[]
                {
                    new TaskItem("Microsoft.VisualStudio.MPF", new Dictionary<string, string>
                    {
                        { "Version", "[11.0,16.0)" },
                        { "DisplayName", "Visual Studio MPF" },
                        { "Source", "Installed" },
                        { "InstallSource", "microsoft.com" },
                    }),
                    new TaskItem("Merq", new Dictionary<string, string>
                    {
                        { "Version", "[1.1.0,2.0.0)" },
                        { "DisplayName", "Extensibility Message Bus" },
                        { "Source", "File" },
                        { "InstallSource", "Embed" },
                        { "VsixSubPath", "SubPath" },
                    }),
                },
                Assets = new ITaskItem[]
                {
                    new TaskItem("Package", new Dictionary<string, string>
                    {
                        { "Type", "Microsoft.VisualStudio.VsPackage" },
                        { "Source", "File" },
                        { "Path", "Package.pkgdef" },
                    }),
                    new TaskItem("MEF", new Dictionary<string, string>
                    {
                        { "Type", "Microsoft.VisualStudio.MefComponent" },
                        { "TargetVersion", "16.0" },
                        { "Source", "Project" },
                        { "ProjectName", "%CurrentProject%" },
                        { "VsixSubPath", "SubPath" },
                        { "Path", "|%CurrentProject%|" },
                    }),
                },
            };

            Assert.True(task.Execute());

            var nav = doc.CreateNavigator();
            var ns = new XmlNamespaceManager(new NameTable());
            ns.AddNamespace("x", XmlNs.NamespaceName);
            ns.AddNamespace("d", XmlNsD.NamespaceName);

            Assert.Equal("TestId", nav.Evaluate("string(/x:PackageManifest/x:Metadata/x:Identity[1]/@Id)", ns));
            Assert.Equal("2.0", nav.Evaluate("string(/x:PackageManifest/x:Metadata/x:Identity[1]/@Version)", ns));
            Assert.Equal("Foo", nav.Evaluate("string(/x:PackageManifest/x:Metadata/x:DisplayName)", ns));
            Assert.Equal("Bar", nav.Evaluate("string(/x:PackageManifest/x:Metadata/x:Description)", ns));

            Assert.Equal(1d, nav.Evaluate("count(/x:PackageManifest/x:Installation/x:InstallationTarget)", ns));
            Assert.Equal(2d, nav.Evaluate("count(/x:PackageManifest/x:Dependencies/x:Dependency)", ns));
            Assert.Equal(1d, nav.Evaluate("count(/x:PackageManifest/x:Prerequisites/x:Prerequisite)", ns));
            Assert.Equal(2d, nav.Evaluate("count(/x:PackageManifest/x:Assets/x:Asset)", ns));

            if (Debugger.IsAttached)
                output.WriteLine(doc.ToString(SaveOptions.None));
        }

        [Fact]
        public void CanDeclareAsset()
        {
            var doc = CreateBasicManifest();

            var task = new UpdateVsixManifest(doc)
            {
                BuildEngine = new MockBuildEngine(output),
                Assets = new ITaskItem[]
                {
                    new TaskItem("Microsoft.VisualStudio.VsPackage", new Dictionary<string, string>
                    {
                        { "Source", "File" },
                        { "Path", "Package.pkgdef" },
                    }),
                    new TaskItem("MEF", new Dictionary<string, string>
                    {
                        { "Type", "Microsoft.VisualStudio.MefComponent" },
                        { "TargetVersion", "16.0" },
                        { "Source", "Project" },
                        { "ProjectName", "%CurrentProject%" },
                        { "VsixSubPath", "SubPath" },
                        { "Path", "|%CurrentProject%|" },
                    }),
                },
            };

            Assert.True(task.Execute());

            var nav = doc.CreateNavigator(true);

            Assert.Equal(2d, nav.Evaluate("count(/PackageManifest/Assets/Asset)"));
            Assert.Equal("Microsoft.VisualStudio.VsPackage", nav.Evaluate("string(/PackageManifest/Assets/Asset[1]/@Type)"));
            Assert.Equal("File", nav.Evaluate("string(/PackageManifest/Assets/Asset[1]/@Source)"));
            Assert.Equal("Package.pkgdef", nav.Evaluate("string(/PackageManifest/Assets/Asset[1]/@Path)"));

            Assert.Equal("Microsoft.VisualStudio.MefComponent", nav.Evaluate("string(/PackageManifest/Assets/Asset[2]/@Type)"));
            Assert.Equal("16.0", nav.Evaluate("string(/PackageManifest/Assets/Asset[2]/@TargetVersion)"));
            Assert.Equal("Project", nav.Evaluate("string(/PackageManifest/Assets/Asset[2]/@Source)"));
            Assert.Equal("%CurrentProject%", nav.Evaluate("string(/PackageManifest/Assets/Asset[2]/@ProjectName)"));
            Assert.Equal("SubPath", nav.Evaluate("string(/PackageManifest/Assets/Asset[2]/@VsixSubPath)"));
            Assert.Equal("|%CurrentProject%|", nav.Evaluate("string(/PackageManifest/Assets/Asset[2]/@Path)"));

            if (Debugger.IsAttached)
                output.WriteLine(doc.ToString(SaveOptions.None));
        }

        XDocument CreateBasicManifest() => 
            new XDocument(new XElement(XmlNs + "PackageManifest",
                new XAttribute(XNamespace.Xmlns + "d", "http://schemas.microsoft.com/developer/vsx-schema-design/2011"),
                new XAttribute("Version", "2.0.0"),
                new XElement(XmlNs + "Metadata",
                    new XElement(XmlNs + "Identity",
                        new XAttribute("Id", "Id"),
                        new XAttribute("Version", "1.0")),
                    new XElement(XmlNs + "DisplayName", "DisplayName"),
                    new XElement(XmlNs + "Description", "Description")
                )
            ));
    }

    public static class XDocumentExtensions
    {
        public static XPathNavigator CreateNavigator(this XDocument document, bool noXmlNamespaces) =>
            noXmlNamespaces ?
            new XPathDocument(new NoXmlNsReader(document.CreateReader())).CreateNavigator() :
            document.CreateNavigator();

        class NoXmlNsReader : Mvp.Xml.Common.XmlWrappingReader
        {
            public NoXmlNsReader(XmlReader baseReader) : base(baseReader) { }

            public override string NamespaceURI => "";
        }
    }
}
