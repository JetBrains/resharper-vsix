using JetBrains.Application;
using JetBrains.Application.Env;
using JetBrains.Application.Extensions;
using JetBrains.DataFlow;
using JetBrains.Util;

namespace JetBrains.ReSharper.Plugins.Vsix2ReSharper.Implementation
{
  [EnvironmentComponent(Sharing.Product)]
  public class VsixExtensionProvider : IExtensionProvider
  {
    private readonly ExtensionLocations extensionLocations;
    private readonly CollectionEvents<IExtension> extensions;

    public VsixExtensionProvider(Lifetime lifetime, ExtensionLocations extensionLocations)
    {
      this.extensionLocations = extensionLocations;
      extensions = new CollectionEvents<IExtension>(lifetime, "VsixExtensionProvider");
    }

    public bool Load(string path)
    {
      // TODO: Logging
      var location = FileSystemPath.Parse(path);
      if (!location.ExistsDirectory)
        return false;

      var loaded = false;
      foreach (var file in location.GetChildFiles("*.nuspec"))
      {
        var nuSpec = NuSpecReader.Read(file);
        if (nuSpec != null)
        {
          extensions.Add(new VsixExtension(this, nuSpec, location, extensionLocations));
          loaded = true;
        }
      }

      return loaded;
    }

    public string Name { get { return "VSIX"; } }
    public IViewable<IExtension> Extensions { get { return extensions; } }
  }
}