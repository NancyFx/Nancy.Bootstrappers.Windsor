## Windsor Bootstrapper for Nancy

This is a bootstrapper for using Windsor with Nancy.  A couple of things are worth noting about this bootstrapper that differ from the others.  First, child containers are a bit of a faux paux with Windsor (if you are really interested you can read a bit about it [here][1], [here][2] and [here][3]) personally I don't fully understand why, but I've found it is virtually impossible to avoid a memory leak if you are creating one for each request.

Fortunately Windsor has something different, a `PerWebRequestLifestyle` which automagically disposes of instances when the web request ends.  Unfortunately it only works with ASP.NET.

You will need to add the following module to your web config however:

```xml
<httpModules>
  <add name="PerWebRequest" type="Castle.MicroKernel.Lifestyle.PerWebRequestLifestyleModule" />
</httpModules>

<modules runAllManagedModulesForAllRequests="true">
  <add name="PerWebRequest" type="Castle.MicroKernel.Lifestyle.PerWebRequestLifestyleModule" />
</modules>
```

If you ave any problems feel free to report bugs here on Github and I will fix them ASAP (it's in my own intrest since I'm using this for work).  Even better would be a pull request with a test ;-).

[1]:http://hammett.castleproject.org/?p=296
[2]:http://kozmic.pl/2010/06/01/castle-windsor-and-child-containers/
[3]:http://kozmic.pl/2010/06/02/castle-windsor-and-child-containers-reloaded/

## Contributors

* [Chris Nicola](http://github.com/lucisferre)
* [Andreas Håkansson](http://github.com/thecodejunkie)
* [Steven Robbins](http://github.com/grumpydev)

## Copyright

Copyright © 2010 Andreas Håkansson, Steven Robbins and contributors

## License

Nancy.Bootstrappers.Windsor is licensed under [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form"). Refer to license.txt for more information.
