## Windsor Bootstrapper for Nancy

This is a bootstrapper for using Windsor with Nancy.  A couple of things are worth noting about this bootstrapper that differ from the others.  First, child containers are a bit of a faux paux with Windsor (if you are really interested you can read a bit about it [here][1], [here][2] and [here][3]) personally I don't fully understand why, but I've found it is virtually impossible to avoid a memory leak if you are creating one for each request.

Instead we use Windsor 3.0's new Scoped Lifestyles. This scopes the resolution of dependencies that use the 'Scoped' lifestyle to the current call scope. Specifically we wrap the call to `INancyEngine.HandleRequest` using a dynamic proxy. When the call finishes the scope is disposed of.

Because the CallContext is not reliable when hosting using ASP.NET and IIS we fall back to the standard PerWebRequest scope if we detect HttpContext.Current. 

To use this when registering your own dependencies simply use the `NancyScopeAccessor` class provided like this:

```csharp
protected override void ConfigureApplicationContainer(IWindsorContainer existingContainer)
{
  // This dependency uses the default singleton lifestyle
  existingContainer.Register(Component.For<IApplicationDependency, ApplicationDependencyClass>());
  // This dependency is registered per-web-request
  existingContainer.Register(Component.For<IRequestDependency, RequestDependencyClass>().LifestyleScoped<NancyPerWebRequestScopeAccessor>());
}
```

See [this page][4] with more details on Windsor's new scoped lifstyle and other new Windsor 3.0 features.

Note that if you are hosting under ASP.NET then you will need to register the Castle PerWebRequestModule. There are two ways to do this:

1. The new 3.0 way: Simply add Microsoft.Web.Infrastructure to your project and it will work automagically (see the WebDemo project for an example)

2. The pre-3.0 way: Add this to your web.config
You will need to add the following module to your web config however:

```xml
<httpModules>
  <add name="PerWebRequest" type="Castle.MicroKernel.Lifestyle.PerWebRequestLifestyleModule" />
</httpModules>

<modules runAllManagedModulesForAllRequests="true">
  <add name="PerWebRequest" type="Castle.MicroKernel.Lifestyle.PerWebRequestLifestyleModule" />
</modules>
```

I highly recommend the new way, as web.config cruft is unsightly.

[1]:http://hammett.castleproject.org/?p=296
[2]:http://kozmic.pl/2010/06/01/castle-windsor-and-child-containers/
[3]:http://kozmic.pl/2010/06/02/castle-windsor-and-child-containers-reloaded/
[4]:http://docs.castleproject.org/Windsor.Whats-New-In-Windsor-3.ashx

## Contributors

* [Chris Nicola](http://github.com/lucisferre)
* [Andreas Håkansson](http://github.com/thecodejunkie)
* [Steven Robbins](http://github.com/grumpydev)

## Copyright

Copyright © 2010 Andreas Håkansson, Steven Robbins and contributors

## License

Nancy.Bootstrappers.Windsor is licensed under [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form"). Refer to license.txt for more information.
