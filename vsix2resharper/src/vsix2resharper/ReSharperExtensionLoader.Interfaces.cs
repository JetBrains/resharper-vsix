using System.Runtime.InteropServices;

// The service is exported to Visual Studio via SReSharperExtensionLoader
// (not by MEF, since you're most likely to call it from your Package.Initialize
// method, and MEF doesn't satisfy Import statements in Package). You can
// cast to IReSharperExtensionLoader to talk to the extension.
// To get these interfaces, either add a reference to the dll, with Embed
// Interop Types set to true, or copy and paste locally. You MUST use the
// same namespace and Guid and ComImport attributes
namespace JetBrains.ReSharper.Plugins.Vsix2ReSharper
{
  [Guid("1DB4BDEF-FF03-45C6-B150-557402D836D4")]
  [ComImport]
  public interface SReSharperExtensionLoader
  {
  }

  [Guid("6C1A9594-7F6F-4E4F-9B42-71B678F2A01E")]
  [ComImport]
  public interface IReSharperExtensionLoader
  {
    bool LoadExtension(string path);
  }
}