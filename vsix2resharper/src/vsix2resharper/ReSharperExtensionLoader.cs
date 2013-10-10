namespace JetBrains.ReSharper.Plugins.Vsix2ReSharper
{
  // TODO: Can we export by MEF? Does it make sense to do so?
  // You most likely want to get this interface during Package.Initialize
  // and it's not going to be available then - MEF doesn't (automatically)
  // wire up Import statements in Package
  internal class ReSharperExtensionLoader : IReSharperExtensionLoader, SReSharperExtensionLoader
  {
    public bool LoadExtension(string path)
    {
      return ReSharperIntegration.Instance.LoadExtension(path);
    }
  }
}