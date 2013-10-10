using System;
using System.Collections.Generic;
using JetBrains.Application.Extensions;
using JetBrains.DataFlow;
using JetBrains.Util;

namespace JetBrains.ReSharper.Plugins.Vsix2ReSharper.Implementation
{
  // TODO: Maybe NuSpec should be merged with these classes?
  public class VsixExtension : IExtension
  {
    private readonly NuSpec nuSpec;
    private readonly FileSystemPath location;
    private readonly ExtensionLocations extensionLocations;

    public VsixExtension(IExtensionProvider provider, NuSpec nuSpec, FileSystemPath location, 
      ExtensionLocations extensionLocations)
    {
      var source = GetType().Name;

      this.nuSpec = nuSpec;
      this.location = location;
      this.extensionLocations = extensionLocations;

      Version = new Version(nuSpec.Version);
      Metadata = new VsixExtensionMetadata(nuSpec);
      Supported = true; // TODO
      Enabled = new Property<bool?>(string.Format("{0}::Enabled", source), null);
      RuntimeInfoRecords = new ListEvents<ExtensionRecord>(string.Format("{0}::RuntimeInfoRecords", source));
      Source = provider;
    }

    public IEnumerable<FileSystemPath> GetFiles(string fileType)
    {
      this.AddInfo(this, string.Format("Looking for '{0}' files in the {1} package folder at {2}", fileType, Id, location.QuoteIfNeeded()));

      foreach (var searchPath in extensionLocations.ExtensionComponentSearchPaths)
      {
        var path = location.Combine(searchPath.Combine(fileType));
        var files = path.GetChildFiles(flags: PathSearchFlags.RecurseIntoSubdirectories);

        ReportFiles(fileType, path, files);

        foreach (var file in files)
          yield return file;
      }
    }

    private void ReportFiles(string fileType, FileSystemPath root, IList<FileSystemPath> files)
    {
      this.AddInfo(this, files.Any()
          ? string.Format("Found {0} files under {1}", files.Count, root)
          : string.Format("The package contains no files in {0}", root));
    }

    public string Id { get { return nuSpec.Id; } }
    public Version Version { get; private set; }
    public string SemanticVersion { get { return nuSpec.Version; } }
    public IExtensionMetadata Metadata { get; private set; }
    public bool Supported { get; private set; }
    public IProperty<bool?> Enabled { get; private set; }
    public ListEvents<ExtensionRecord> RuntimeInfoRecords { get; private set; }
    public IExtensionProvider Source { get; private set; }

    public class VsixExtensionMetadata : IExtensionMetadata
    {
      private readonly NuSpec nuSpec;

      public VsixExtensionMetadata(NuSpec nuSpec)
      {
        this.nuSpec = nuSpec;

        Authors = nuSpec.Authors;
        Owners = nuSpec.Owners;
        Tags = nuSpec.Tags;

        IconUrl = nuSpec.IconUrl;
        LicenseUrl = nuSpec.LicenseUrl;
        ProjectUrl = nuSpec.ProjectUrl;

        Created = null;
        PreRelease = nuSpec.Version == Version.Parse(nuSpec.Version).ToString();
      }

      public string Title { get { return nuSpec.Title; } }
      public string Description { get { return nuSpec.Description; } }
      public string Summary { get { return nuSpec.Summary; } }
      public string Copyright { get { return nuSpec.Copyright; } }
      public IEnumerable<string> Authors { get; private set; }
      public IEnumerable<string> Owners { get; private set; }
      public IEnumerable<string> Tags { get; private set; }
      // TODO: Set dependencies
      public IEnumerable<string> DependencyIds { get; private set; }
      public IEnumerable<string> DependencyDescriptions { get; private set; }
      public Uri IconUrl { get; private set; }
      public Uri LicenseUrl { get; private set; }
      public Uri ProjectUrl { get; private set; }
      public DateTimeOffset? Created { get; private set; }
      public bool PreRelease { get; private set; }
    }
  }
}