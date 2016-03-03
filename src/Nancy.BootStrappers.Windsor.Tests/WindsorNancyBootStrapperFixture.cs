namespace Nancy.Bootstrappers.Windsor.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using Castle.Windsor;
    using Nancy.BootStrappers.Windsor.Tests.Fakes;
    using Nancy.Tests;
    using Xunit;

    public class WindsorNancyBootstrapperFixture
    {
        private readonly FakeWindsorNancyBootstrapper bootstrapper;

        public WindsorNancyBootstrapperFixture()
        {
            this.bootstrapper = new FakeWindsorNancyBootstrapper();
            this.bootstrapper.Initialise();
        }

        [Fact]
        public void Should_be_able_to_get_engine_when_providing_application_container_exteranlly()
        {
            // Given
            var bootstrapperWithExternalContainer = new FakeWindsorNancyBootstrapper(new WindsorContainer());
            bootstrapperWithExternalContainer.Initialise();

            // When
            var result = bootstrapperWithExternalContainer.GetEngine();

            // Then
            result.ShouldNotBeNull();
            result.ShouldBeOfType<INancyEngine>();
        }

        [Fact]
        public void Should_be_able_to_get_engine_instance()
        {
            // Given
            // When
            var result = this.bootstrapper.GetEngine();

            // Then
            result.ShouldNotBeNull();
            result.ShouldBeOfType<INancyEngine>();
        }

        [Fact]
        public void Should_configure_application_container_when_getting_engine()
        {
            // Given
            // When
            this.bootstrapper.GetEngine();

            // Then
            this.bootstrapper.ApplicationContainerConfigured.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_be_able_to_make_request_from_module_with_no_dependency()
        {
            // Given
            var engine = this.bootstrapper.GetEngine();

            // When
            var ctx = await engine.HandleRequest(new Request("GET", "/fake/route/with/some/parts", "http"));

            // Then
            ctx.Response.StatusCode.ShouldEqual(Nancy.HttpStatusCode.OK);
            ctx.Dispose();
        }

        [Fact]
        public async Task Should_be_able_to_make_request_from_module_with_dependencies()
        {
            // Given
            var engine = this.bootstrapper.GetEngine();

            // When
            var ctx = await engine.HandleRequest(new Request("GET", "/with-dependency", "http"));

            // Then
            ctx.Response.StatusCode.ShouldEqual(Nancy.HttpStatusCode.OK);
            ctx.Dispose();
        }

        [Fact]
        public async Task Should_be_able_to_make_simultaneous_requests_use_different_module_instances()
        {
            // Given
            var engine = this.bootstrapper.GetEngine();
            var ctx1 = await engine.HandleRequest(new Request("GET", "/fake/unique", "http"));
            var ctx2 = await engine.HandleRequest(new Request("GET", "/fake/unique", "http"));

            // When
            var response1 = ctx1.Response.GetContentsAsString();
            var response2= ctx2.Response.GetContentsAsString();

            // Then
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

            for (var i = 0; i < 1000000; i++)
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