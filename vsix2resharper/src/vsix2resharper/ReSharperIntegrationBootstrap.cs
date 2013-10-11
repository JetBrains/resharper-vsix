using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using JetBrains.Vsix.ReSharperIntegration;
using Microsoft.VisualStudio.Shell.Interop;

using IVsOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace JetBrains.ReSharper.Plugins.Vsix2ReSharper
{
  // TODO: How do I export this properly, so this package can be delay loaded?
  // Or, perhaps the question is how can I import it into another package?
  //[Export(typeof(IReSharperIntegrationBootstrap))]
  internal class ReSharperIntegrationBootstrap : SReSharperIntegrationBootstrap
  {
    private const string ReSharperIntegrationAssemblyName = "JetBrains.ReSharper.Plugins.Vsix2ReSharper.Implementation";

    private readonly IVsShell vsShell;
    private readonly IVsOleServiceProvider vsOleServiceProvider;

    [ImportingConstructor]
    public ReSharperIntegrationBootstrap(IVsShell vsShell, IVsOleServiceProvider vsOleServiceProvider)
    {
      this.vsShell = vsShell;
      this.vsOleServiceProvider = vsOleServiceProvider;

      int fInstalled;
      IsReSharperInstalled = vsShell.IsPackageInstalled(GuidList.ReSharperPkg, out fInstalled) == 0 &&
                             Convert.ToBoolean(fInstalled);
    }

    public bool IsReSharperInstalled { get; private set; }

    public void Initialise(ReSharperIntegrationCallback callback)
    {
      var package = GetReSharperPackage();
      if (package == null)
        return;

      var version = GetReSharperVersion(package);
      var assembly = LoadIntegrationAssembly(version);
      if (assembly == null)
      {
        // TODO: Should we really throw here?
        // Perhaps we should return an enum - NotInstalled, NotSupported, Initialising, Initialised?
        throw new NotSupportedException(string.Format("Unable to load integration assembly for ReSharper {0}", version));
      }

      var bootstrap = assembly.GetType("Bootstrap");
      var initialise = bootstrap.GetMethod("Initialise");
      ReSharperApiImplementationCallback onImplementationInitialised = implementation => callback(new ReSharperIntegration(version, implementation));
      initialise.Invoke(null, new object[] { vsOleServiceProvider, onImplementationInitialised });
    }

    private IVsPackage GetReSharperPackage()
    {
      if (!IsReSharperInstalled)
        return null;

      IVsPackage package;
      if (vsShell.IsPackageLoaded(GuidList.ReSharperPkg, out package) != 0)
        vsShell.LoadPackage(GuidList.ReSharperPkg, out package);

      return package;
    }

    private static Version GetReSharperVersion(IVsPackage package)
    {
      return package.GetType().Assembly.GetName().Version;
    }

    private Assembly LoadIntegrationAssembly(Version resharperVersion)
    {
      // TODO: Do I need the resolver?
      using (new AssemblyResolver())
      {
        var path = Path.GetDirectoryName(GetType().Assembly.Location);
        var assemblyName = string.Format("{0}.{1}", ReSharperIntegrationAssemblyName, resharperVersion.ToString(2));
        var name = new AssemblyName(assemblyName) { CodeBase = path };
        return Assembly.Load(name);
      }
    }
  }
}
