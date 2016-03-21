#if !__MonoCS__
namespace Nancy.Bootstrappers.Windsor.Tests
{
    using Bootstrapper;
    using Bootstrappers.Windsor;
    using Bootstrappers.Windsor.Tests;
    using Castle.Windsor;
    using Nancy.Tests.Unit.Bootstrapper.Base;

    public class BoostrapperBaseFixture : BootstrapperBaseFixtureBase<IWindsorContainer>
    {
        private readonly WindsorNancyBootstrapper bootstrapper;

        public BoostrapperBaseFixture()
        {
            this.bootstrapper = new FakeWindsorNancyBootstrapper(this.Configuration);
        }

        protected override NancyBootstrapperBase<IWindsorContainer> Bootstrapper
        {
            get { return this.bootstrapper; }
        }

        public override void Should_use_types_from_config()
        {
            // This test is not included for the WindsorNancyBoostrapper because it uses an
            // interceptor, to provide a custom lifetime, which means that we'll actually
            // be getting back a proxy to the FakeEngine.
        }
    }
}
#endif