namespace Nancy.Bootstrappers.Windsor.Tests
{
    using BootStrappers.Windsor.Tests.Fakes;
    using Bootstrapper;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;

    public class FakeWindsorNancyBootstrapper : WindsorNancyBootstrapper
    {
        public IWindsorContainer Container
        {
            get { return this.ApplicationContainer; }
        }

        public bool ApplicationContainerConfigured { get; set; }

        public bool RequestContainerConfigured { get; set; }

        protected override void ApplicationStartup(IWindsorContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            this.RequestContainerConfigured = true;
        }

        protected override void ConfigureApplicationContainer(IWindsorContainer existingContainer)
        {
            this.ApplicationContainerConfigured = true;
            base.ConfigureApplicationContainer(existingContainer);

            existingContainer.Register(
                Component.For<Foo, IFoo>(),
                Component.For<FakeDependency, IDependency>()
                );
        }
    }
}