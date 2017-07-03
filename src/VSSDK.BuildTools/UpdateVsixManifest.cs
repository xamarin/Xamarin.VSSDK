using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xamarin.VSSDK.Properties;
using System.Xml.Linq;
using System.Linq;
using System.Collections;
using System.Xml;
using System.ComponentModel;
using System.Collections.Generic;

namespace Xamarin.VSSDK
{
    /// <summary>
    /// Updates a VSIX manifest with information specified via the MSBuild project.
    /// </summary>
    public class UpdateVsixManifest : Task
    {
        /// <summary>
        /// The source manifest to update.
        /// </summary>
        [Required]
        public ITaskItem SourceVsix { get; set; }

        /// <summary>
        /// The target manifest file to write with the updates.
        /// </summary>
        [Required]
        [Output]
        public ITaskItem TargetVsix { get; set; }

        /// <summary>
        /// VSIX manifest metadata.
        /// </summary>
        public ITaskItem Metadata { get; set; }

        /// <summary>
        /// VSIX installation metadata.
        /// </summary>
        public ITaskItem Installation { get; set; }

        /// <summary>
        /// Installation targets for the VSIX.
        /// </summary>
        public ITaskItem[] InstallationTargets { get; set; }

        /// <summary>
        /// Dependencies for the VSIX.
        /// </summary>
        public ITaskItem[] Dependencies { get; set; }

        /// <summary>
        /// Prerequisites for the VSIX.
        /// </summary>
        public ITaskItem[] Prerequisites { get; set; }

        /// <summary>
        /// Assets provided by the VSIX.
        /// </summary>
        public ITaskItem[] Assets { get; set; }

        XDocument sourceVsixManifestDocument;

        public UpdateVsixManifest() { }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public UpdateVsixManifest(XDocument sourceVsixManifest) => sourceVsixManifestDocument = sourceVsixManifest;

        static readonly HashSet<string> designAttributes = new HashSet<string>(new[] { "ProjectName", "Source", "InstallSource", "VsixSubPath" });

        static readonly XNamespace XmlNs = XNamespace.Get("http://schemas.microsoft.com/developer/vsx-schema/2011");
        static readonly XNamespace XmlNsDesign = XNamespace.Get("http://schemas.microsoft.com/developer/vsx-schema-design/2011");

        /// <summary>
        /// Updates the <see cref="SourceVsix"/> with the given 
        /// information and writes it to <see cref="TargetVsix"/>.
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            if (sourceVsixManifestDocument == null && !File.Exists(SourceVsix.GetMetadata("FullPath")))
            {
                Log.LogErrorCode(nameof(Strings.UpdateVsixManifest.XVS001), Strings.UpdateVsixManifest.XVS001(SourceVsix));
                return false;
            }

            var doc = sourceVsixManifestDocument ?? XDocument.Load(SourceVsix.GetMetadata("FullPath"));

            string value;
            var metadata = doc.Root.Element(XmlNs + "Metadata");
            if (metadata == null)
            {
                metadata = new XElement(XmlNs + "Metadata");
                doc.Root.AddFirst(metadata);
            }

            if (Metadata != null)
            {
                if (!string.IsNullOrEmpty(value = Metadata.GetMetadata("DisplayName")))
                    metadata.Element(XmlNs + "DisplayName")?.SetValue(value);
                if (!string.IsNullOrEmpty(value = Metadata.GetMetadata("Description")))
                    metadata.Element(XmlNs + "Description")?.SetValue(value);
                if (!string.IsNullOrEmpty(value = Metadata.GetMetadata("MoreInfo")))
                    metadata.Element(XmlNs + "MoreInfo")?.SetValue(value);
                if (!string.IsNullOrEmpty(value = Metadata.GetMetadata("License")))
                    metadata.Element(XmlNs + "License")?.SetValue(value);
                if (!string.IsNullOrEmpty(value = Metadata.GetMetadata("GettingStartedGuide")))
                    metadata.Element(XmlNs + "GettingStartedGuide")?.SetValue(value);
                if (!string.IsNullOrEmpty(value = Metadata.GetMetadata("ReleaseNotes")))
                    metadata.Element(XmlNs + "ReleaseNotes")?.SetValue(value);
                if (!string.IsNullOrEmpty(value = Metadata.GetMetadata("Icon")))
                    metadata.Element(XmlNs + "Icon")?.SetValue(value);
                if (!string.IsNullOrEmpty(value = Metadata.GetMetadata("PreviewImage")))
                    metadata.Element(XmlNs + "PreviewImage")?.SetValue(value);
                if (!string.IsNullOrEmpty(value = Metadata.GetMetadata("Tags")))
                    metadata.Element(XmlNs + "Tags")?.SetValue(value);

                var identity = metadata.Element(XmlNs + "Identity");
                identity.SetAttributeValue("Id", Metadata.ItemSpec);
                // Can't use CopyAttributes because some metadata attributes already went to elements above.
                if (!string.IsNullOrEmpty(value = Metadata.GetMetadata("Version")))
                    identity.SetAttributeValue("Version", value);
                if (!string.IsNullOrEmpty(value = Metadata.GetMetadata("Language")))
                    identity.SetAttributeValue("Language", value);
                if (!string.IsNullOrEmpty(value = Metadata.GetMetadata("Publisher")))
                    identity.SetAttributeValue("Publisher", value);
            }

            var installation = doc.Root.Element(XmlNs + nameof(Installation));
            if (installation == null &&
                (Installation != null || InstallationTargets?.Length > 0))
            {
                installation = new XElement(XmlNs + nameof(Installation));
                metadata.AddAfterSelf(installation);
            }

            if (Installation != null)
                CopyAttributes(Installation, installation);

            AddOrUpdate(InstallationTargets, installation, "InstallationTarget");

            if (Prerequisites?.Length > 0)
            {
                var prerequisites = doc.Root.Element(XmlNs + nameof(Prerequisites));
                if (prerequisites == null)
                {
                    prerequisites = new XElement(XmlNs + nameof(Prerequisites));
                    metadata.AddAfterSelf(prerequisites);
                }

                AddOrUpdate(Prerequisites, prerequisites, "Prerequisite");
            }

            if (Dependencies?.Length > 0)
            {
                var dependencies = doc.Root.Element(XmlNs + nameof(Dependencies));
                if (dependencies == null)
                {
                    dependencies = new XElement(XmlNs + nameof(Dependencies));
                    metadata.AddAfterSelf(dependencies);
                }

                AddOrUpdate(Dependencies, dependencies, "Dependency");
            }

            if (Assets?.Length > 0)
            {
                var assets = doc.Root.Element(XmlNs + nameof(Assets));
                if (assets == null)
                {
                    assets = new XElement(XmlNs + nameof(Assets));
                    doc.Root.Add(assets);
                }

                var nsd = XNamespace.Get("http://schemas.microsoft.com/developer/vsx-schema-design/2011");
                foreach (var item in Assets)
                {
                    var asset = new XElement(XmlNs + "Asset");
                    assets.Add(asset);
                    // We default the Type of asset to the Include attribute.
                    if (string.IsNullOrEmpty(item.GetMetadata("Type")))
                        asset.Add(new XAttribute("Type", item.ItemSpec));

                    CopyAttributes(item, asset);
                }
            }

            if (sourceVsixManifestDocument == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(TargetVsix.GetMetadata("FullPath")));
                using (var writer = XmlWriter.Create(TargetVsix.GetMetadata("FullPath"), new XmlWriterSettings { Indent = true }))
                    doc.Save(writer);
            }

            return true;
        }

        void AddOrUpdate(ITaskItem[] items, XElement parent, string elementName)
        {
            if (items?.Length > 0)
            {
                foreach (var item in items)
                {
                    var element = parent.Elements(parent.Name.Namespace + elementName).FirstOrDefault(x => x.Attribute("Id")?.Value == item.ItemSpec);
                    if (element == null)
                    {
                        element = new XElement(parent.Name.Namespace + elementName, new XAttribute("Id", item.ItemSpec));
                        parent.Add(element);
                    }
                    CopyAttributes(item, element);
                }
            }
        }

        void CopyAttributes(ITaskItem item, XElement element)
        {
            foreach (DictionaryEntry entry in item.CloneCustomMetadata())
            {
                var name = (string)entry.Key;
                if (designAttributes.Contains(name))
                    element.SetAttributeValue(XmlNsDesign + name, (string)entry.Value);
                else
                    element.SetAttributeValue(name, (string)entry.Value);
            }
        }
    }
}
