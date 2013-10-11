using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using IVsOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace JetBrains.ReSharper.Plugins.Vsix2ReSharper
{
  // TODO: Stop being auto load
  [PackageRegistration(UseManagedResourcesOnly = true)]
  [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
  [Guid(GuidList.PkgString)]
  [ProvideAutoLoad(UIContextGuids.NoSolution)]
  public sealed class VSPackage : Package
  {
    protected override void Initialize()
    {
      var vsShell = (IVsShell)GetService(typeof(IVsShell));
      var vsOleServiceProvider = (IVsOleServiceProvider)GetService(typeof(IVsOleServiceProvider));

      // TODO: Logging
      var serviceContainer = (IServiceContainer)this;
      var bootstrapper = new ReSharperIntegrationBootstrap(vsShell, vsOleServiceProvider);
      serviceContainer.AddService(typeof(SReSharperIntegrationBootstrap), bootstrapper, true);
      serviceContainer.AddService(typeof(IReSharperIntegrationBootstrap), bootstrapper, true);

      base.Initialize();
    }

    protected override void Dispose(bool disposing)
    {
      ((IServiceContainer) this).RemoveService(typeof (SReSharperIntegrationBootstrap));
      ((IServiceContainer) this).RemoveService(typeof (IReSharperIntegrationBootstrap));

      base.Dispose(disposing);
    }
  }
}

