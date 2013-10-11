# VSIX and ReSharper integration #

This repo contains two projects that enable better integration between Visual Studio's VSIX extensions and ReSharper.

The first project, `resharper-vsix` is a ReSharper extension that will install any VSIX files bundled inside one of ReSharper 8's NuGet based extensions. This is intended to enable ReSharper extensions to also extend Visual Studio in ways that ReSharper doesn't currently support, perhaps by including project or item templates, or different editor extensions (highlights, adornments, folding, etc).

The second project, `vsix2resharper` is a lightweight Visual Studio extension which exposes a service that other VSIX extensions can use to easily load bundled ReSharper extensions. This allows VSIX extensions to optionally support ReSharper, if it's installed, by bundling plugins, settings or external annotations. So, a VSIX can integrate with ReSharper, e.g. by tracking when the code completion intellisense window is visible, or by adding quick-fixes and context actions, or providing Live Templates or Structural Search and Replace patterns.

Using either method, a ReSharper extension can request a MEF interface exposed by a VSIX extension, and use this to enable communication and integration between ReSharper and VSIX extensions.

Since both VSIX and ReSharper extensions support dependencies, both extensions are easy to install as a dependency to either your VSIX extension or NuGet based ReSharper extension.

## Project status ##

Consider this an alpha release, to solicit feedback. It works, but isn't quite feature complete. Don't use this in production yet - the API WILL change.

The packages are not yet available on the respective galleries, but will be added shortly, pending some review.

### Known issues ###

The following items still need addressing. Not tricky, just haven't done them yet:

1. Testing! Especially `resharper-vsix`.
2. Implement logging
3. Uninstall VSIX when uninstalling a ReSharper extension
4. Samples, both loading/installing and communicating
5. Support for VS2008 and VS2010. Currently built against the VS2013 SDK. No technical reason.

## resharper-vsix ##

This is a ReSharper extension that in turn extends ReSharper's own extensibility mechanism. When a new ReSharper extension is installed, this extension looks for any VSIX files deployed in any of the following folders of the package, in this order:

    ReSharper\v8.0\vsix\vs12.0\*.vsix
    ReSharper\v8.0\vsix\*.vsix
    ReSharper\vAny\vsix\*.vsix
    DotNet\v1.0\vsix\*.vsix
    DotNet\vAny\vsix\*.vsix

(Where `vs12.0` is the version of the current Visual Studio instance, and `v8.0` is the current ReSharper version. These will differ depending on your runtime configuration.)

When it finds a VSIX, the extension will invoke Visual Studio's extension manager to install the extension. This usually requires Visual Studio to be restarted (since Visual Studio extensions are statically registered), and ReSharper's extension manager will prompt for restart if this is the case.

When the ReSharper extension is uninstalled, the VSIX will also be uninstalled (not currently implemented), and again, you will be prompted for restart if required.

> Note that ReSharper already provides many extension points to integrate with Visual Studio - for example, tool windows, menus and property extenders. You can also retrieve any Visual Studio interface from the Component Model (although this can often mean making your ReSharper extension Visual Studio version specific). You should not use this extension if ReSharper already provides what you need.

To communicate with your VSIX extension, it is best to have the VSIX export interfaces via MEF, and consumed by the ReSharper extension. ReSharper can retrieve MEF interfaces, and Visual Studio's component manager will ensure the VSIX is loaded before your ReSharper extension can use the interfaces.

> If you require your VSIX extension to call into the ReSharper extension, it is still best to initiate integration by exporting interfaces from the VSIX and consuming from the ReSharper extension. This ensures all dependencies are loaded in the right order. The exposed interface can be an initialisation interface that accepts the interfaces exposed by the ReSharper extension.

You can register the MEF interfaces with ReSharper's Component Model, which means you can then import them via a component's constructor parameter. See the ReSharper dev guide's [Component Model page](http://confluence.jetbrains.com/display/NETCOM/2.02+Component+Model+%28R8%29) for more details.

    [WrapVsInterfaces]
    public class ExposeMefInterfaces : IExposeVsServices
    {
      public void Register(VsServiceProviderResolver.VsServiceMap map)
      {
        map.OptionalMef<IMyMefInterface>();

        // or importing an interface registered as a service 
        // if (!map.IsRegistered<IComponentModel>())
        //  map.QueryService<SComponentModel>().As<IComponentModel>();
      }
    }

    [ShellComponent]
    public class MyReSharperComponent
    {
      // ReSharper is a .net 3.5 application. It doesn't support .net's Lazy<T>
      public MyReSharperComponent(JetBrains.Util.Lazy.Lazy<IMyMefInterface> injected)
      {
        // Make sure injected is created - someone might have disabled the VSIX!
        if (injected.IsCreated)
        {
          // ...
        }
      }
    }

> The `map.Mef<T>` method can also be used to register interfaces instead of `map.OptionalMef<T>`, but you need to be sure that the interface will ALWAYS be available. If it's not available, perhaps because the user has disabled or uninstalled the VSIX, then the Component Model will throw an exception, which can cause cascading failures within ReSharper.

You should make your ReSharper extension's NuGet package depend on the `resharper-vsix` package. This means adding a line to the `.nuspec` file:

    <package>
      <metadata>
        [...]
        <dependencies>
          <dependency id="ReSharper" version="8.0" />
          <dependency id="ReSharper.Vsix" version="1.0" />
        </dependencies>
        [...]
      </metadata>
    </package>

> Note that until the package is uploaded to the ReSharper plugin gallery, the ID and version of the package are not yet defined. They will likely change. 

## vsix2resharper ##

`vsix2resharper` is a VSIX extension. Once installed, it exposes a service that other VSIX extensions can use to load ReSharper extensions that are bundled inside the calling VSIX. It is very lightweight and loads a single, small, assembly and does nothing until someone requests the service. The actual ReSharper integration is implemented in a second, version dependent assembly that isn't loaded until the service is requested and isn't loaded at all if ReSharper isn't installed.

Once the service is initialised, a VSIX can load a ReSharper extension by calling the `LoadExtension` method on the API. This will load a ReSharper extension from disk. It does not install the extension into the central repository. Instead, the extension is considered to be installed at the location given, and is loaded from that location each time Visual Studio is started, in the same way that centrally installed extensions are loaded. For this reason, the ReSharper extension should not be a `.nupkg` package file, but should be distributed as loose files, using the same [folder structure](http://confluence.jetbrains.com/display/NETCOM/1.9+Packaging+%28R8%29) as a `.nupkg` file, and should include a `.nuspec` file for metadata. The path passed to `LoadExtension` is the path to the folder containing the `.nuspec` file. It is not the path to the `.nuspec` itself.

> The extension isn't installed centrally because its lifetime is tightly coupled to and controlled by the lifetime of the owning VSIX. If the extension were installed, it would be very difficult to uninstall when the VSIX is uninstalled, and would complicate updates - would they come from a new version of the VSIX, or from the ReSharper gallery? What would happen if the ReSharper extension were updated out-of-sync with the VSIX extension? What if it were uninstalled?

Once the ReSharper extension has been loaded, it is listed in the "Installed" section of the ReSharper extension manager. It cannot be uninstalled, as that is the responsibility of the owning VSIX (since the VSIX is responsible for loading the extension, when the VSIX is uninstalled, the extension is no longer loaded). However, it can still be disabled, should the user not want it running.

### Initialising the service ###

Initialising the integration with ReSharper is a potentially asynchronous operation, depending on the thread you are calling from, and whether ReSharper is already initialised. This requires a simple two step process - retrieving a "bootstrap" service, calling `Initialise` and passing in a callback which receives the initialised API.

> Generally speaking, if you call the service from your `Package.Initialise` method, the initialisation will complete synchronously.

The code looks like this:

      string pathToExtension = GetPathToExtension(); 
      var bootstrap = GetService(typeof (SReSharperIntegrationBootstrap)) as IReSharperIntegrationBootstrap;
      if (bootstrap != null && bootstrap.IsReSharperInstalled)
      {
        bootstrap.Initialise(resharper => resharper.LoadExtension(pathToExtension));
      }

The VSIX can check to see if ReSharper is installed with the `IsReSharperInstalled` property. The callback will not be executed if ReSharper isn't installed, or the version of ReSharper isn't supported. Currently, only ReSharper 8.0 is supported. Support for 7.1 is intended.

> TODO: There is currently no feedback from the `Initialise` method, e.g. NotInstalled, NotSupported, Initialising, Initialised. I think this will change - currently it throws if ReSharper isn't supported.

### The API ###

The API makes use of [.net 4's type equivalence](http://msdn.microsoft.com/en-us/library/dd997297.aspx). This means you can either reference the main assembly with "Embed Interop Types" set to `true`, or paste the following into your code:

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

As long as the `Guid`, `ComImport` and `TypeIdentifier` attributes are kept in place, .net will consider your versions of these interfaces as the same as those in the implementing assembly, and allow casting between them.

There are a few advantages to using type equivalence to define the API, such as not requiring a binary assembly reference, and the versioning concerns that come with that, and it also allows for relocating the implementation of the interface in the future. The interface can be safely updated in future revisions by adding new methods or properties to the end of the interface.

The downside is that type equivalence doesn't work with generic types, such as `Action<IReSharperIntegration>` or `Task<IReSharperIntegration>`, so a custom delegate is required for the callback. Fortunately, the C# compiler will handle instantiating it from an anonymous lambda.

> TODO: The bootstrap interface should expose an API version to show what methods are supported. Default to 1.

### Communicating with the ReSharper extension ###

The best way to implement communication is by exposing MEF interfaces from your VSIX and consuming them from the ReSharper extension. The ReSharper extension can implement Components that are created at application startup, and can immediately call into the MEF interfaces, passing in callback interfaces to allow for two way communication.

See the `resharper-vsix` section above for consuming MEF interfaces.

### Packaging ###

This extension will shortly be listed on the Visual Studio gallery. Once that is the case, your VSIX can take a dependency on this VSIX, and it will be automatically installed by the Visual Studio extension manager. The dependency is easily added to the `.vsixmanifest` file through the manifest editor - simply select `vsix2resharper` as a dependency. Alternatively, add the following to the file in the text editor:

    <PackageManifest ...>
      [...]
      <Dependencies>
        [...]
        <Dependency d:Source="Installed"
                    Id=92a3389f-ac48-4fcc-8450-0e918428d944"
                    DisplayName="vsix2resharper"
                    Version="[1.0,2.0)"
                    d:InstallSource="Download" />
      </Dependencies>
      [...]
    </PackageManifest>

## Building ##

Both extensions support only ReSharper 8.0 at this time. They require the [ReSharper 8 SDK](http://www.jetbrains.com/resharper/download/) to be installed. `vsix2resharper` also requires the Visual Studio SDK (currently VS2013, VS2010 support is planned).

> The currently downloadable SDK is the 8.0.1 SDK. It has some extra interfaces that were introduced as part of 8.0.1. If you install this SDK, your extension will require ReSharper 8.0.1, due to the binary references to the 8.0.1 assemblies. The published versions of these extensions will be built against the original 8.0 SDK, which you can still download from [the archives page](http://resharper-support.jetbrains.com/entries/21206508-Where-can-I-download-an-old-previous-ReSharper-version-).

## Why two extensions? ##

The two extensions are very similar, and do appear to solve the same issue - integrating VSIX extensions and ReSharper. However, they are aimed at different usages. Which one to use depends on where the primary integration is - are you integrating with Visual Studio or ReSharper?

If your extension is a Visual Studio extension, and you want to provide better support or new features if and when ReSharper is installed, use ``vsix2resharper``. The VSIX is the primary integration point, and controls the lifetime of the ReSharper extension. Without the VSIX, the ReSharper extension cannot function. The ReSharper extension becomes the value added proposition to the main VSIX extension.

On the other hand, if your extension is a ReSharper extension, that uses ReSharper's deep knowledge of the codebase, or provides refactorings, analysis and quick-fixes, but also wants to extend Visual Studio to e.g. display a new editor adornment or classifier, then you should use `resharper-vsix`. The ReSharper extension is the primary extension, and controls the lifetime of the VSIX.