using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using IVsOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace JetBrains.ReSharper.Plugins.Vsix2ReSharper
{
  [PackageRegistration(UseManagedResourcesOnly = true)]
  [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
  [Guid(GuidList.PkgString)]
  [ProvideAutoLoad(UIContextGuids.NoSolution)]
  public sealed class VSPackage : Package
  {
    private IntegrationLoader integrationLoader;

    protected override void Initialize()
    {
      var vsShell = (IVsShell)GetService(typeof(IVsShell));
      var vsOleServiceProvider = (IVsOleServiceProvider)GetService(typeof(IVsOleServiceProvider));

      // TODO: Logging
      // TODO: Lazily instantiate so we don't load the integration dll until needed
      integrationLoader = new IntegrationLoader(vsShell, vsOleServiceProvider);
      integrationLoader.Initialise();

      var serviceContainer = (IServiceContainer) this;
      var loader = new ReSharperExtensionLoader();
      serviceContainer.AddService(typeof(SReSharperExtensionLoader), loader, true);
      serviceContainer.AddService(typeof(IReSharperExtensionLoader), loader, true);

      base.Initialize();
    }

    protected override void Dispose(bool disposing)
    {
      integrationLoader.Dispose();

      base.Dispose(disposing);
    }
  }
}

