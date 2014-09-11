namespace Nancy.Bootstrappers.Windsor.Tests
{
    using BootStrappers.Windsor.Tests.Fakes;
    using Bootstrapper;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;

    public class FakeWindsorNancyBootstrapper : WindsorNancyBootstrapper
    {
      private readonly WindsorContainer externalContainer;
      private readonly NancyInternalConfiguration configuration;

        public bool ApplicationContainerConfigured { get; set; }

        public bool RequestContainerConfigured { get; set; }

        public FakeWindsorNancyBootstrapper()
            : this(configuration: null)
        {
        }

        public FakeWindsorNancyBootstrapper(NancyInternalConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public FakeWindsorNancyBootstrapper(WindsorContainer windsorContainer)
        {
            this.externalContainer = windsorContainer;
        }

        protected override IWindsorContainer GetApplicationContainer()
        {
             return externalContainer ?? base.GetApplicationContainer();
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
                Component.For<FakeDependency, IDependency>()
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