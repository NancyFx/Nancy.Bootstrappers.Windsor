## Windsor Bootstrapper for Nancy

This is a bootstrapper for using Windsor with Nancy.  A couple of things are worth noting about this bootstrapper that differ from the others.  First, child containers are a bit of a faux paux with Windsor (if you are really interested you can read a bit about it [here][1], [here][2] and [here][3]) personally I don't fully understand why, but I've found it is virtually impossible to avoid a memory leak if you are creating one for each request.

Instead we use Windsor 3.0's new Scoped Lifestyle. This scopes the resolution of dependencies that use the 'Scoped' lifestyle to the current call scope. Specifically we wrap the call to `INancyEngine.HandleRequest` using a dynamic proxy. When the call finishes the scope is disposed of.

This way we no longer need to rely on the `PerWebRequest` lifestyle which only supports ASP.NET hosted applications. You are now free to use Windsor and host Nancy any way you like.

Note: If you have additional dependencies/services that you would like scoped to the request (rather than per-application as singletons) you simply need to register them with `Lifstyle.Scoped()`. See [this page][4] with more details on Windsor's new scoped lifstyle and other new Windsor 3.0 features.

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
