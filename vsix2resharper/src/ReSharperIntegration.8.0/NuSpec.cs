using System;
using System.Collections.Generic;

namespace JetBrains.ReSharper.Plugins.Vsix2ReSharper.Implementation
{
  public class NuSpec
  {
    public NuSpec()
    {
      Dependencies = new List<Tuple<string, string>>();
    }

    public string Id;
    public string Version;
    public string Title;
    public IEnumerable<string> Authors;
    public IEnumerable<string> Owners;
    public Uri LicenseUrl;
    public Uri ProjectUrl;
    public Uri IconUrl;
    public string Description;
    public string Summary;
    public string ReleaseNotes;
    public string Copyright;
    public IEnumerable<string> Tags;
    public IList<Tuple<string, string>> Dependencies;

    public void AddDependency(string id, string versionSpec)
    {
      Dependencies.Add(Tuple.Create(id, versionSpec));
    }
  }
}