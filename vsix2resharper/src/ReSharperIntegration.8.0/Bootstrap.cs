using JetBrains.Application.Components;
using JetBrains.DataFlow;
using JetBrains.ReSharper.Plugins.Vsix2ReSharper.Implementation;
using JetBrains.Threading;
using JetBrains.VsIntegration.Application;
using JetBrains.Vsix.ReSharperIntegration;

using IVsOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

// ReSharper disable once CheckNamespace
public class Bootstrap
{
  private static LifetimeDefinition lifetimeDefinition;

  public static void Initialise(IVsOleServiceProvider vsServiceProvider, ReSharperApiImplementationCallback callback)
  {
    // TODO: Is the guard strictly necessary?
    // dotCover uses it, but we always seem to get executed immediately
    ReentrancyGuard.Current.ExecuteOrQueue("Register VSIX extension provider",
      () =>
      {
        var jetEnvironment = JetVisualStudioHost.GetOrCreateHost(vsServiceProvider).Environment;
        var catalogue = PartsCatalogueFactory.Create(typeof(Bootstrap).Assembly);
        lifetimeDefinition = Lifetimes.Define(EternalLifetime.Instance, "VsixExtensionProvider");
        jetEnvironment.FullPartCatalogSet.Add(lifetimeDefinition.Lifetime, catalogue);

        var extensionProvider = jetEnvironment.Container.GetComponent<VsixExtensionProvider>();

        callback(new ReSharperApiImplementation(extensionProvider));
      });
  }

  public static void Dispose()
  {
    ReentrancyGuard.Current.ExecuteOrQueue("Unregister VSIX extension provider", () =>
    {
      if (lifetimeDefinition != null)
        lifetimeDefinition.Terminate();
    });
  }

  private class ReSharperApiImplementation : IReSharperApiImplementation
  {
    private readonly VsixExtensionProvider extensionProvider;

    public ReSharperApiImplementation(VsixExtensionProvider vsixExtensionProvider)
    {
      extensionProvider = vsixExtensionProvider;
    }

    public bool LoadExtension(string path)
    {
      return extensionProvider != null && extensionProvider.LoadExtension(path);
    }
  }
}