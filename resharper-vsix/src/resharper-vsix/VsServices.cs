using JetBrains.VsIntegration.Application;
using Microsoft.VisualStudio.ExtensionManager;

namespace JetBrains.ReSharper.Plugins.VsixIntegration
{
  public class VsServices : IExposeVsServices
  {
    public void Register(VsServiceProviderResolver.VsServiceMap map)
    {
      map.QueryService<SVsExtensionManager>().As<IVsExtensionManager>();
    }
  }
}