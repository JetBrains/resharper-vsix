using System;
using System.IO;
using System.Reflection;
using JetBrains.Vsix.ReSharperIntegration;
using Microsoft.VisualStudio.Shell.Interop;

using IVsOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace JetBrains.ReSharper.Plugins.Vsix2ReSharper
{
  internal class IntegrationLoader : IDisposable
  {
    private const string ReSharperIntegrationAssemblyName = "JetBrains.ReSharper.Plugins.Vsix2ReSharper.Implementation";

    private readonly IVsShell vsShell;
    private readonly IVsOleServiceProvider vsOleServiceProvider;

    private Type bootstrap;

    public IntegrationLoader(IVsShell vsShell, IVsOleServiceProvider vsOleServiceProvider)
    {
      this.vsShell = vsShell;
      this.vsOleServiceProvider = vsOleServiceProvider;
    }

    public void Initialise()
    {
      // TODO: Logging
      ReSharperIntegration.Initialise(LoadIntegrationImplementation());
    }

    private IReSharperIntegration LoadIntegrationImplementation()
    {
      var assembly = LoadIntegrationAssembly();
      bootstrap = assembly.GetType("Bootstrap");
      return GetImplementation();
    }

    private IReSharperIntegration GetImplementation()
    {
      var initialise = bootstrap.GetMethod("Initialise");
      return initialise != null
        ? initialise.Invoke(null, new object[] { vsOleServiceProvider }) as IReSharperIntegration
        : null;
    }

    private Assembly LoadIntegrationAssembly()
    {
      var resharperVersion = GetReSharperVersion();
      var path = Path.GetDirectoryName(GetType().Assembly.Location);
      if (resharperVersion == null || path == null)
        return null;

      // TODO: Do I need the resolver?
      using (new AssemblyResolver())
      {
        var assemblyName = string.Format("{0}.{1}", ReSharperIntegrationAssemblyName, resharperVersion.ToString(2));
        var name = new AssemblyName(assemblyName) { CodeBase = path };
        return Assembly.Load(name);
      }
    }

    private Version GetReSharperVersion()
    {
      var package = GetReSharperPackage();
      return package != null ? package.GetType().Assembly.GetName().Version : null;
    }

    private object GetReSharperPackage()
    {
      IVsPackage package = null;
      int fInstalled;
      if (vsShell.IsPackageInstalled(GuidList.ReSharperPkg, out fInstalled) == 0 && Convert.ToBoolean(fInstalled))
      {
        if (vsShell.IsPackageLoaded(GuidList.ReSharperPkg, out package) == 0)
          return package;
        vsShell.LoadPackage(GuidList.ReSharperPkg, out package);
      }
      return package;
    }

    public void Dispose()
    {
      if (bootstrap != null)
      {
        var dispose = bootstrap.GetMethod("Dispose");
        if (dispose != null)
          dispose.Invoke(null, new object[0]);
        bootstrap = null;
      }
    }
  }
}