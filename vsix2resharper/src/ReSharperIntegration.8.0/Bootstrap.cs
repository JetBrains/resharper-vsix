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

  public static IReSharperIntegration Initialise(IVsOleServiceProvider vsServiceProvider)
  {
    var integration = new Integration();

    // TODO: Do I really need this? dotCover uses it...
    ReentrancyGuard.Current.ExecuteOrQueue("Register VSIX extension provider",
      () =>
      {
        var jetEnvironment = JetVisualStudioHost.GetOrCreateHost(vsServiceProvider).Environment;
        var catalogue = PartsCatalogueFactory.Create(typeof(Bootstrap).Assembly);
        lifetimeDefinition = Lifetimes.Define(EternalLifetime.Instance, "VsixExtensionProvider");
        jetEnvironment.FullPartCatalogSet.Add(lifetimeDefinition.Lifetime, catalogue);

        var extensionProvider = jetEnvironment.Container.GetComponent<VsixExtensionProvider>();

        integration.SetExtensionProvider(extensionProvider);
      });

    return integration;
  }

  public static void Dispose()
  {
    ReentrancyGuard.Current.ExecuteOrQueue("Unregister VSIX extension provider", () =>
    {
      if (lifetimeDefinition != null)
        lifetimeDefinition.Terminate();
    });
  }

  private class Integration : IReSharperIntegration
  {
    private VsixExtensionProvider extensionProvider;

    public bool LoadExtension(string path)
    {
      return extensionProvider != null && extensionProvider.Load(path);
    }

    internal void SetExtensionProvider(VsixExtensionProvider provider)
    {
      extensionProvider = provider;
    }
  }
}