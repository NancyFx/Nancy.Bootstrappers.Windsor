namespace Nancy.Bootstrappers.Windsor.Tests
{
    using BootStrappers.Windsor.Tests.Fakes;
    using Bootstrapper;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;

    public class FakeWindsorNancyBootstrapper : WindsorNancyBootstrapper
    {
        private readonly NancyInternalConfiguration configuration;

        public bool ApplicationContainerConfigured { get; set; }

        public bool RequestContainerConfigured { get; set; }

        public FakeWindsorNancyBootstrapper()
            : this(null)
        {
        }

        public FakeWindsorNancyBootstrapper(NancyInternalConfiguration configuration)
        {
            this.configuration = configuration;
        }

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
                Component.For<FakeDependency, IDependency>().LifestyleBoundToNearest<INancyModule>()
                );
        }

        public IWindsorContainer Container
        {
            get { return this.ApplicationContainer; }
        }

        protected override NancyInternalConfiguration InternalConfiguration
        {
            get { return this.configuration ?? base.InternalConfiguration; }
        }
    }
}