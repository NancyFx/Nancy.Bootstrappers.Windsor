using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Windsor;

namespace WebDemo
{
    public class DemoBootstrapper : WindsorNancyBootstrapper
    {
        // Overriding this just to show how it works, not actually necessary as autoregister
        // takes care of it all.
        protected override void ConfigureApplicationContainer(IWindsorContainer existingContainer)
        {
            // We don't call base because we don't want autoregister
            // we just register our one known dependency as an application level singleton
            existingContainer.Register(Component.For<IApplicationDependency, ApplicationDependencyClass>());
            existingContainer.Register(Component.For<IRequestDependency, RequestDependencyClass>().LifestyleScoped<NancyPerWebRequestScopeAccessor>());
        }

        protected override void ApplicationStartup(IWindsorContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
        }
    }
}