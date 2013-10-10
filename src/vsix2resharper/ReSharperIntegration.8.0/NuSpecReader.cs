using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Util;
using JetBrains.Util.Logging;

namespace JetBrains.ReSharper.Plugins.Vsix2ReSharper.Implementation
{
  public static class NuSpecReader
  {
    private static readonly IDictionary<string, Action<NuSpec, string>> StringActions = new Dictionary<string,Action<NuSpec,string>>
    {
      {"id", (spec, content) => spec.Id = content },
      {"version", (spec, content) => spec.Version = content },
      {"title", (spec, content) => spec.Title = content },
      {"authors", (spec, content) => spec.Authors = SplitSemiColonString(content) },
      {"owners", (spec, content) => spec.Owners = SplitSemiColonString(content) },
      {"licenseUrl", (spec, content) => spec.LicenseUrl = CreateUri(content) },
      {"projectUrl", (spec, content) => spec.ProjectUrl = CreateUri(content) },
      {"iconUrl", (spec, content) => spec.IconUrl = CreateUri(content) },
      {"description", (spec, content) => spec.Description = content },
      {"summary", (spec, content) => spec.Summary = content },
      {"releaseNotes", (spec, content) => spec.ReleaseNotes = content },
      {"copyright", (spec, content) => spec.Copyright = content },
      {"tags", (spec, content) => spec.Tags = SplitSemiColonString(content) },
    };

    public static NuSpec Read(FileSystemPath path)
    {
      if (!path.ExistsFile)
        return null;

      NuSpec nuspec = null;

      Logger.CatchSilent(() =>
      {
        using (var stream = path.OpenFileForReading())
        {
          stream.ReadXml(packageReader =>
          {
            if (!packageReader.ReadToDescendant("metadata"))
              return;

            packageReader.ReadElement(metadataReader =>
            {
              if (metadataReader.LocalName != "metadata")
                return;

              nuspec = new NuSpec();
              metadataReader.ReadSubElements(childReader =>
              {
                Action<NuSpec, string> action;
                if (StringActions.TryGetValue(childReader.LocalName, out action))
                  action(nuspec, childReader.ReadElementContentAsString());
              });
            });
          });
        }
      });

      return nuspec;
    }

    private static IEnumerable<string> SplitSemiColonString(string content)
    {
      return content.Split(';').Select(s => s.Trim()).ToList();
    }

    private static Uri CreateUri(string content)
    {
      Uri uri;
      Uri.TryCreate(content, UriKind.Absolute, out uri);
      return uri;
    }
  }
}