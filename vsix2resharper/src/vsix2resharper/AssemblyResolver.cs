using System;
using System.Linq;
using System.Reflection;

namespace JetBrains.ReSharper.Plugins.Vsix2ReSharper
{
  internal class AssemblyResolver : IDisposable
  {
    public AssemblyResolver()
    {
      AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
    }

    public void Dispose()
    {
      AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
    }

    private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
      var currentDomainAssemblyResolve = (from a in AppDomain.CurrentDomain.GetAssemblies()
        where a.FullName == args.Name || a.GetName().Name == args.Name
        select a).FirstOrDefault();
      return currentDomainAssemblyResolve;
    }
  }
}