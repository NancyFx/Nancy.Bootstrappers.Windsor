using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Nancy.BootStrappers.Windsor.Tests.Fakes;
using Nancy.Bootstrapper;
using Nancy.Tests;
using Xunit;

namespace Nancy.Bootstrappers.Windsor.Tests
{
    public static class Helpers
    {
        public static string GetContentsAsString(this Response r)
        {
            var stream = new MemoryStream();
            r.Contents.Invoke(stream);
            stream.Position = 0;
            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }


    public class FakeWindsorNancyBootstrapper : WindsorNancyBootstrapper
    {
        public bool ApplicationContainerConfigured { get; set; }

        public IWindsorContainer Container
        {
            get { return this.ApplicationContainer; }
        }

        public bool RequestContainerConfigured { get; set; }

        protected override void ApplicationStartup(IWindsorContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            RequestContainerConfigured = true;

            container.Register(
                Component.For<Foo, IFoo>(),
                Component.For<FakeDependency, IDependency>()
            );
        }

        protected override void ConfigureApplicationContainer(IWindsorContainer existingContainer)
        {
            ApplicationContainerConfigured = true;
            base.ConfigureApplicationContainer(existingContainer);
        }
    }

    public class WindsorNancyBootstrapperFixture
    {
        private readonly FakeWindsorNancyBootstrapper bootstrapper;

        public WindsorNancyBootstrapperFixture()
        {
            this.bootstrapper = new FakeWindsorNancyBootstrapper();
            this.bootstrapper.Initialise();
        }

        [Fact]
        public void the_bootstrapper_returns_an_instance_of_INancyEngine()
        {
            var result = this.bootstrapper.GetEngine();
            result.ShouldNotBeNull();
            result.ShouldBeOfType<INancyEngine>();
        }

        [Fact]
        public void GetEngine_ConfigureApplicationContainer_Should_Be_Called()
        {
            this.bootstrapper.GetEngine();
            this.bootstrapper.ApplicationContainerConfigured.ShouldBeTrue();
        }

        [Fact]
        public void GetAllModules_Returns_As_MultiInstance()
        {
            this.bootstrapper.GetEngine();
            var context = new NancyContext();
            var output1 = this.bootstrapper.GetAllModules(context).FirstOrDefault(nm => nm.GetType() == typeof(FakeNancyModuleWithBasePath));
            var output2 = this.bootstrapper.GetAllModules(context).FirstOrDefault(nm => nm.GetType() == typeof(FakeNancyModuleWithBasePath));

            output1.ShouldNotBeNull();
            output2.ShouldNotBeNull();
            output1.ShouldNotBeSameAs(output2);
        }

        [Fact]
        public void GetModuleByKey_Returns_As_MultiInstance()
        {
            this.bootstrapper.GetEngine();
            var context = new NancyContext();
            var output1 = this.bootstrapper.GetModuleByKey(typeof(FakeNancyModuleWithBasePath).FullName, context);
            var output2 = this.bootstrapper.GetModuleByKey(typeof(FakeNancyModuleWithBasePath).FullName, context);

            output1.ShouldNotBeNull();
            output2.ShouldNotBeNull();
            output1.ShouldNotBeSameAs(output2);
        }

        [Fact]
        public void GetEngine_Defaults_Registered_In_Container()
        {
            this.bootstrapper.GetEngine();
            var defaults = NancyInternalConfiguration.WithOverrides(x => 
            {
                x.ModuleKeyGenerator = typeof(WindsorModuleKeyGenerator);
            });
            foreach (var registration in defaults.GetTypeRegistations().Where(x => x.RegistrationType != typeof(INancyEngine)))
            {
                this.bootstrapper.Container.Resolve(registration.RegistrationType)
                    .ShouldBeOfType(registration.ImplementationType);
            }
            // NancyEngine is being proxied by Castle to intercept the HandleRequest call

            this.bootstrapper.Container.Resolve<INancyEngine>()
                .GetType().Name.ShouldEqual("INancyEngineProxy");
        }

        [Fact]
        public void can_make_request_from_module_with_no_dependency()
        {
            var engine = this.bootstrapper.GetEngine();
            var ctx = engine.HandleRequest(new Request("GET", "/fake/route/with/some/parts", "http"));
            ctx.Response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            ctx.Dispose();
        }

        [Fact]
        public void can_make_request_from_module_with_dependencies()
        {
            var engine = this.bootstrapper.GetEngine();
            var ctx = engine.HandleRequest(new Request("GET", "/with-dependency", "http"));
            ctx.Response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            ctx.Dispose();
        }

        [Fact]
        public void simultaneous_requests_use_different_module_instances()
        {
            var engine = this.bootstrapper.GetEngine();
            var ctx1 = engine.HandleRequest(new Request("GET", "/fake/unique", "http"));
            var ctx2 = engine.HandleRequest(new Request("GET", "/fake/unique", "http"));
            var response1 = ctx1.Response.GetContentsAsString();
            var response2= ctx2.Response.GetContentsAsString();
            response1.ShouldNotEqual(response2);
            ctx1.Dispose();
            ctx2.Dispose();
        }


        [Fact(Skip = "For testing memory leaks only")]
        public void Check_windsor_memory_leak()
        { 
            var engine = this.bootstrapper.GetEngine();
            var ctx = engine.HandleRequest(new Request("GET", "/fake/route/with/some/parts", "http"));
            ctx.Dispose();
            Console.WriteLine("Start - " + GC.GetTotalMemory(false).ToString("#,###,##0") + " Bytes");
            for (int i = 0; i < 1000000; i++)
            {
                engine = this.bootstrapper.GetEngine();
                ctx = engine.HandleRequest(new Request("GET", "/fake/route/with/some/parts", "http"));
                ctx.Dispose();
            }
            Console.WriteLine("End - " + GC.GetTotalMemory(false).ToString("#,###,##0") + " Bytes");
        }

        [Fact(Skip = "For testing memory leaks with ASP.NET hosting only")]
        public void Check_windsor_memory_leak_with_aspnet_hosting()
        { 
            var tasks = new List<Task>(100000);
            for (var i = 0; i < 100000; i++)
            {
                tasks.Add(Task.Factory.StartNew(() => {
                    try {
                    var request = new WebClient();
                    request.DownloadData("http://localhost/WebDemo/");
                    request.DownloadData("http://localhost/WebDemo/dependency2/");
                    request.DownloadData("http://localhost/WebDemo/dependency1/");
                    } catch(Exception ex) {}
                }));
            }
            Task.WaitAll(tasks.ToArray());
        }
    }
}