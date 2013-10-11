using System;
using System.Runtime.InteropServices;

// Make use of .net 4's type equivalence feature. You can either reference this
// assembly, with Embed Interop Types set to true, or paste these types into
// the source of your own VSIX, and even though they are different types, .net
// will treat them as equal
//
// Sadly, .net doesn't like Action<TCustom> or Task<TCustom>, hence the annoying
// delegate for the callback (and we need the callback because the initialisation
// might be async, depending on what thread we're called and if ReSharper is fully
// initialised yet)
namespace JetBrains.ReSharper.Plugins.Vsix2ReSharper
{
  [TypeIdentifier("vsix2resharper", "bootstrap_callback")]
  public delegate void ReSharperIntegrationCallback(IReSharperIntegration resharper);

  [Guid("D57719A7-721E-44D1-8299-A8D936305D37")]
  [ComImport]
  [TypeIdentifier("vsix2resharper", "boostrap")]
  public interface IReSharperIntegrationBootstrap
  {
    bool IsReSharperInstalled { get; }
    void Initialise(ReSharperIntegrationCallback callback);
  }

  [Guid("6D80D40B-CF81-43A8-A638-B81131DBD089")]
  [ComImport]
  [TypeIdentifier("vsix2resharper", "integration")]
  public interface IReSharperIntegration
  {
    Version ReSharperVersion { get; }
    bool LoadExtension(string folderPath);
  }

  [Guid("6C258F67-BB65-420A-8173-29022084DED2")]
  [ComImport]
  [TypeIdentifier("vsix2resharper", "bootstrap_service")]
  public interface SReSharperIntegrationBootstrap : IReSharperIntegrationBootstrap
  {
  }
}