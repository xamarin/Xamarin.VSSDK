using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace VsixTemplate
{
	[Guid("01A7700B-4990-4C4D-821D-9C2591AB9D70")]
	[PackageRegistration(UseManagedResourcesOnly = true)]
	public sealed class VsixTemplatePackage : Package
	{
		protected override void Initialize()
		{
		}
	}
}