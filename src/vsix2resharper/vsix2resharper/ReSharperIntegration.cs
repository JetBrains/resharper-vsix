using JetBrains.Vsix.ReSharperIntegration;

namespace JetBrains.ReSharper.Plugins.Vsix2ReSharper
{
  internal class ReSharperIntegration
  {
    private readonly IReSharperIntegration implementation;
    internal static ReSharperIntegration Instance { get; private set; }

    internal static void Initialise(IReSharperIntegration implementation)
    {
      Instance = new ReSharperIntegration(implementation);
    }

    private ReSharperIntegration(IReSharperIntegration implementation)
    {
      this.implementation = implementation;
    }

    public bool LoadExtension(string path)
    {
      return implementation != null && implementation.LoadExtension(path);
    }
  }
}