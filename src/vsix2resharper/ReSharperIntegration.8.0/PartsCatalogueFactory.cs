using System.Linq;
using System.Reflection;
using JetBrains.Application.Env;
using JetBrains.Application.Parts;
using JetBrains.Metadata.Reader.API;
using JetBrains.Util;

namespace JetBrains.ReSharper.Plugins.Vsix2ReSharper.Implementation
{
  public static class PartsCatalogueFactory
  {
    public static IPartsCatalogue Create(params Assembly[] assemblies)
    {
      var assemblyPaths = (from assembly in assemblies
        select FileSystemPath.Parse(assembly.Location)).ToArray();
      return Create(assemblyPaths);
    }

    private static IPartsCatalogue Create(params FileSystemPath[] assemblyPaths)
    {
      var resharperInstallDirectory = typeof(JetEnvironment).Assembly.GetPath().Directory;
      return new MetadataPartsCatalogue(assemblyPaths, new FlyweightPartFactory(),
        lifetime => new DefaultAssemblyResolver(assemblyPaths.Concat(resharperInstallDirectory)));
    }
  }
}