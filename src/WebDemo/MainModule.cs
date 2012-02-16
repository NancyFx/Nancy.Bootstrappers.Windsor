namespace Nancy.Demo.Bootstrappers.Windsor
{
    using System;
    using Nancy;
    using Nancy.Routing;

    public class MainModule : NancyModule
    {
        public MainModule(IRouteCacheProvider routeCacheProvider)
        {
            Get["/"] = x => "Hello World it's " + DateTime.Now.ToLongTimeString();

            Get["/filtered", r => true] = x => "This is a route with a filter that always returns true.";

            Get["/filtered", r => false] = x => "This is also a route, but filtered out so should never be hit.";

            Get[@"/(?<foo>\d{2,4})/{bar}"] = x => string.Format("foo: {0}<br/>bar: {1}", x.foo, x.bar);

            Get["/test"] = x => "Test";

            Get["/error"] = x =>
                {
                    throw new NotSupportedException("This is an exception thrown in a route.");
                };
        }
    }
}
