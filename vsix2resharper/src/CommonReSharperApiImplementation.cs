using System.Runtime.InteropServices;

// Shared interface between the VSPackage and the ReSharper integration
// implementation dlls. The Guid, ComImport, and Type.FullName (i.e.
// including namespace) mark this type as available for type equivalence.
// The assembly also needs the Guid and ImportedFromTypeLib attributes
namespace JetBrains.Vsix.ReSharperIntegration
{
  [Guid("61E571A7-E437-4096-880D-EF15C17568A3")]
  [ComImport]
  [TypeIdentifier("vsix2resharper_api", "implementation")]
  public interface IReSharperApiImplementation
  {
    bool LoadExtension(string path);
  }

  [TypeIdentifier("vsix2resharper_api", "callback")]
  public delegate void ReSharperApiImplementationCallback(IReSharperApiImplementation implementation);
}