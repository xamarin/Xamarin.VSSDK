using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xamarin.VSSDK.Properties;

namespace Xamarin.VSSDK
{
    /// <summary>
    /// Verifies if the vsix contains assembly references that might not be
    /// supported by the target VS version
    /// </summary>
    public class ValidateVsixReferencedAssemblies : Task
    {
        readonly Func<string, IEnumerable<AssemblyName>> referencedAssembliesProvider;

        [Required]
        public string Dev { get; set; }

        [Required]
        public ITaskItem[] VsixSourceItems { get; set; }

        [Required]
        public ITaskItem[] ReferencedAssembliesToValidate { get; set; }

        public ITaskItem[] ExcludeValidateReferencedAssemblies { get; set; }

        public ValidateVsixReferencedAssemblies()
            : this(assemblyFile => Assembly.ReflectionOnlyLoadFrom(assemblyFile).GetReferencedAssemblies())
        { }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public ValidateVsixReferencedAssemblies(
            Func<string, IEnumerable<AssemblyName>> referencedAssembliesProvider) =>
                this.referencedAssembliesProvider = referencedAssembliesProvider;

        public override bool Execute()
        {
            var devVersion = Version.Parse(Dev);

            if (ExcludeValidateReferencedAssemblies == null)
                ExcludeValidateReferencedAssemblies = Array.Empty<ITaskItem>();

            var assemblyFiles = VsixSourceItems
                .Select(x => x.ItemSpec)
                .Where(x => ".dll".Equals(Path.GetExtension(x), StringComparison.OrdinalIgnoreCase));

            foreach (var assemblyFile in assemblyFiles)
            {
                try
                {
                    var conflicts = referencedAssembliesProvider(assemblyFile)
                        .Where(x =>
                            ShouldValidateAssemblyReference(x) &&
                            x.Version.Major > devVersion.Major);

                    foreach (var conflict in conflicts)
                        Log.LogErrorCode(
                            nameof(Strings.ValidateVsixReferencedAssemblies.XVS002),
                            Strings.ValidateVsixReferencedAssemblies.XVS002(
                                Path.GetFileNameWithoutExtension(assemblyFile),
                                conflict.FullName,
                                Dev));
                }
                catch (BadImageFormatException) { }
                catch (FileLoadException) { }
                catch (Exception ex)
                {
                    Log.LogWarningFromException(ex);
                }
            }

            return !Log.HasLoggedErrors;
        }

        bool ShouldValidateAssemblyReference(AssemblyName reference) =>
            ReferencedAssembliesToValidate.Any(x => reference.FullName.Contains(x.ItemSpec)) &&
            !ExcludeValidateReferencedAssemblies.Any(x => reference.FullName.Contains(x.ItemSpec));
    }
}