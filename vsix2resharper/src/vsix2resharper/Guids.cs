using System;

namespace JetBrains.ReSharper.Plugins.Vsix2ReSharper
{
  internal static class GuidList
  {
    public const string PkgString = "51b25d22-c67e-424d-b813-2cee65391e04";

    // This string is from typeof(ReSharperPkg).Guid
    public static readonly Guid ReSharperPkg = new Guid("0C6E6407-13FC-4878-869A-C8B4016C57FE");
  }
}