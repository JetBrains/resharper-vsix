using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("vsix2resharper")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("JetBrains")]
[assembly: AssemblyProduct("vsix2resharper")]
[assembly: AssemblyCopyright("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]   
[assembly: ComVisible(true)]     
[assembly: CLSCompliant(false)]
[assembly: NeutralResourcesLanguage("en-US")]

[assembly: AssemblyVersion("0.1.0.*")]

// Enable the magic for type equivalence
// http://blogs.msdn.com/b/mshneer/archive/2008/10/28/advances-in-net-type-system-type-equivalence-demo.aspx
[assembly: ImportedFromTypeLib("Vsix2ReSharper")]
[assembly: Guid("2FBE9323-1230-4E49-B172-AF333C8AB34B")]
