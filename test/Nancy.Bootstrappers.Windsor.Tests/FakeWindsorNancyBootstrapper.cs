namespace Nancy.Bootstrappers.Windsor.Tests
{
    using System;
    using Fakes;
    using Bootstrapper;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;

    public class FakeWindsorNancyBootstrapper : WindsorNancyBootstrapper
    {
      private readonly WindsorContainer externalContainer;
      private readonly Func<ITypeCatalog, NancyInternalConfiguration> configuration;

        public bool ApplicationContainerConfigured { get; set; }

        public bool RequestContainerConfigured { get; set; }

        public FakeWindsorNancyBootstrapper()
            : this(configuration: null)
        {
        }

        public FakeWindsorNancyBootstrapper(Func<ITypeCatalog, NancyInternalConfiguration> configuration)
        {
            this.configuration = configuration;
        }

        public FakeWindsorNancyBootstrapper(WindsorContainer windsorContainer)
        {
            this.externalContainer = windsorContainer;
        }

        protected override IWindsorContainer GetApplicationContainer()
        {
             return this.externalContainer ?? base.GetApplicationContainer();
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

        protected override Func<ITypeCatalog, NancyInternalConfiguration> InternalConfiguration
        {
            get { return this.configuration ?? base.InternalConfiguration; }
        }
    }

    public class FakeStartupTask : IRequestStartup
    {
        public void Initialize(IPipelines pipelines, NancyContext context)
        {
        }
    }
}
