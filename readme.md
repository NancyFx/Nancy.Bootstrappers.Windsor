## Windsor Bootstrapper for NancyThis is a bootstrapper for using Windsor with Nancy.  A couple of things are worth noting about this bootstrapper that differ from the others.  First, child containers are a bit of a faux paux with Windsor (if you are really interested you can read a bit about it [here][1], [here][2] and [here][3]) personally I don't fully understand why, but I've found it is virtually impossible to avoid a memory leak if you are creating one for each request.In a previous version a scoped lifestyle was used. This scoped lifestyle used a dynamic proxy to wrap the call to `INancyEngine.HandleRequest` inside a scope. This didn't work with the async version of `HandleRequest` that was used in OWIN.Since the only dependency that is resolved per Request is the `INancyModule` we now wrap the resolved Module in an `IDisposable` class that will Release the module from the container. This class is added to the `INancyContext.Items`. When the `INancyContext` is disposed, all disposable items are also disposed. At this point, the container will Release the module.If you need a dependency that is bound to the current request, you can use Windsor 3.2's `BoundToNearest<INancyModule>`. This will bind depencencies to the nearest INancyModule. This means that there will only be one instance of this dependency in the lifetime of an `INancyModule`To use this when registering your own dependencies simply register your dependency like this:```c#protected override void ConfigureApplicationContainer(IWindsorContainer existingContainer){  // This dependency uses the default singleton lifestyle  existingContainer.Register(Component.For<IApplicationDependency, ApplicationDependencyClass>());    // This dependency is registered per-web-request  existingContainer.Register(Component.For<IRequestDependency, RequestDependencyClass>().LifestyleBoundToNearest<INancyModule>());}```[1]:http://hammett.castleproject.org/?p=296[2]:http://kozmic.pl/2010/06/01/castle-windsor-and-child-containers/[3]:http://kozmic.pl/2010/06/02/castle-windsor-and-child-containers-reloaded/## UsageWhen Nancy detects that the `WindsorNancyBootstrapper` type is available in the AppDomain of your application, it will assume you want to use it, rather than the default one.The easiest way to get the latest version of `WindsorNancyBootstrapper` into your application is to install the `Nancy.Boostrappers.Windsor` nuget.### CustomizingBy inheriting from `WindsorNancyBootstrapper` you will gain access to the `IWindsorContainer` of the application and request containers and can perform what ever reqistations that your application requires.```c#public class Bootstrapper : WindsorNancyBootstrapper{    protected override void ApplicationStartup(IWindsorContainer container, IPipelines pipelines)    {        // No registrations should be performed in here, however you may        // resolve things that are needed during application startup.    }    protected override void ConfigureApplicationContainer(IWindsorContainer existingContainer)    {        // Perform registrations here    }    protected override void RequestStartup(IWindsorContainer container, IPipelines pipelines, NancyContext context)    {        // No registrations should be performed in here, however you may        // resolve things that are needed during request startup.    }}```You can also override the `GetApplicationContainer` method and return a pre-existing container instance, instead of having Nancy create one for you. This is useful if Nancy is co-existing with another application and you want them to share a single container.```c#protected override IWindsorContainer GetApplicationContainer(){    // Return application container instance}```## Contributors* [Chris Nicola](http://github.com/lucisferre)* [Andreas Håkansson](http://github.com/thecodejunkie)* [Steven Robbins](http://github.com/grumpydev)## CopyrightCopyright © 2010 Andreas Håkansson, Steven Robbins and contributors## LicenseNancy.Bootstrappers.Windsor is licensed under [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form"). Refer to license.txt for more information.