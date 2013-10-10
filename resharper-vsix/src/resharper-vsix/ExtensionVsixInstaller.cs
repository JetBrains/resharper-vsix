using System;
using System.Collections.Generic;
using JetBrains.Application;
using JetBrains.Application.Extensions;
using JetBrains.DataFlow;
using JetBrains.Util;
using Microsoft.VisualStudio.ExtensionManager;

namespace JetBrains.ReSharper.Plugins.VsixIntegration
{
  [ShellComponent]
  public class ExtensionVsixInstaller : IExtensionRepository
  {
    private readonly HashSet<string> extensionsRequiringRestart = new HashSet<string>();

    public ExtensionVsixInstaller(Lifetime lifetime, ExtensionManager extensionManager,
      IVsExtensionManager vsExtensionManager)
    {
      extensionManager.SupportedExtensions.View(lifetime, (extensionLifetime, extension) =>
      {
        foreach (var file in extension.GetFiles("vsix"))
        {
          var vsix = vsExtensionManager.CreateInstallableExtension(file.FullPath);
          if (vsix != null && !vsExtensionManager.IsInstalled(vsix) &&
              vsExtensionManager.Install(vsix, false) != RestartReason.None)
          {
            extensionsRequiringRestart.Add(string.Format("{0}.{1}", extension.Id, extension.SemanticVersion));
          }
        }
      });
    }

    public bool CanUninstall(string id)
    {
      // We're not responsible for uninstalling the extension
      return false;
    }

    public void Uninstall(string id, bool removeDependencies, IEnumerable<string> dependencies,
      Action<LoggingLevel, string> logger)
    {
      // TODO: Uninstall any vsix from the id, or its dependencies
    }

    public bool HasMissingExtensions()
    {
      return false;
    }

    public void RestoreMissingExtensions()
    {
    }

    public IEnumerable<string> GetExtensionsRequiringRestart()
    {
      return extensionsRequiringRestart;
    }
  }
}