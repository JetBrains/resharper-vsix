using System;
using JetBrains.Vsix.ReSharperIntegration;

namespace JetBrains.ReSharper.Plugins.Vsix2ReSharper
{
  internal class ReSharperIntegration : IReSharperIntegration
  {
    private readonly IReSharperApiImplementation implementation;

    public ReSharperIntegration(Version version, IReSharperApiImplementation implementation)
    {
      ReSharperVersion = version;
      this.implementation = implementation;
    }

    public Version ReSharperVersion { get; private set; }
    public bool LoadExtension(string folderPath)
    {
      return implementation.LoadExtension(folderPath);
    }
  }
}
