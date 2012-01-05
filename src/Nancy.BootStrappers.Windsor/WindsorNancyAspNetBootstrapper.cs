using System;
using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Nancy.BootStrappers.Windsor;
using Nancy.Bootstrapper;
using Nancy.Routing;

namespace Nancy.Bootstrappers.Windsor
{
    /// <summary>
    /// This does not create a child container because in Windsor this leads to memory leaks.  Instead be sure to use
    /// PerWebRequest or Transient lifestyle for anything that needs to be disposed after each request.  
    /// </summary>
    public abstract class WindsorNancyAspNetBootstrapper : NancyBootstrapperBase<IWindsorContainer>
    {
        bool _modulesRegistered;

        protected override IEnumerable<IApplicationStartup> GetApplicationStartupTasks()
        {
            return this.ApplicationContainer.ResolveAll<IApplicationStartup>();
        }

        protected override IEnumerable<IRequestStartup> GetRequestStartupTasks(IWindsorContainer container) {
            return container.ResolveAll<IRequestStartup>();
        }

        protected override INancyEngine GetEngineInternal()
        {
            return this.ApplicationContainer.Resolve<INancyEngine>();
        }

        protected override IModuleKeyGenerator GetModuleKeyGenerator() {
            return this.ApplicationContainer.Resolve<IModuleKeyGenerator>();
        }

        protected override IWindsorContainer GetApplicationContainer()
        {
            if (this.ApplicationContainer != null) return this.ApplicationContainer;
            var container = new WindsorContainer();
            container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel, true));
            return container;
        }

        protected override void RegisterModules(IWindsorContainer container, IEnumerable<ModuleRegistration> moduleRegistrationTypes)
        {
            if (_modulesRegistered) return;
            _modulesRegistered = true;
            IEnumerable<ComponentRegistration<object>> components = moduleRegistrationTypes
                .Select(r => Component.For(typeof (NancyModule))
                .ImplementedBy(r.ModuleType)
                .Named(r.ModuleKey)
                .LifeStyle.Custom<HybridPerWebRequestLifestyleManager<TransientLifestyleManager>>());
            this.ApplicationContainer.Register(components.ToArray());
        }

        public override IEnumerable<NancyModule> GetAllModules(NancyContext context)
        {
            return this.ApplicationContainer.ResolveAll<NancyModule>();
        }

        public override NancyModule GetModuleByKey(string moduleKey, NancyContext context)
        {
            return this.ApplicationContainer.Resolve<NancyModule>(moduleKey);
        }

        protected override void RegisterBootstrapperTypes(IWindsorContainer applicationContainer)
        {
            applicationContainer.Register(Component.For<INancyModuleCatalog>().Instance(this));
        }

        protected override void RegisterTypes(IWindsorContainer container, IEnumerable<TypeRegistration> typeRegistrations)
        {
            IEnumerable<ComponentRegistration<object>> components = typeRegistrations
                .Where(t => t.RegistrationType != typeof (IModuleKeyGenerator))
                .Select(r => Component.For(r.RegistrationType)
                    .ImplementedBy(r.ImplementationType));
            container.Register(components.ToArray());
            container.Register(Component.For<IModuleKeyGenerator>().ImplementedBy<WindsorModuleKeyGenerator>());
            container.Register(Component.For<Func<IRouteCache>>()
                .UsingFactoryMethod(ctx => (Func<IRouteCache>) (ctx.Resolve<IRouteCache>)));
        }

        protected override void RegisterCollectionTypes(IWindsorContainer container, IEnumerable<CollectionTypeRegistration> collectionTypeRegistrations)
        {
            foreach (CollectionTypeRegistration collectionTypeRegistration in collectionTypeRegistrations)
            {
                foreach (Type implementationType in collectionTypeRegistration.ImplementationTypes)
                    container.Register(Component.For(collectionTypeRegistration.RegistrationType).ImplementedBy(implementationType));
            }
        }

        protected override void RegisterInstances(IWindsorContainer container, IEnumerable<InstanceRegistration> instanceRegistrations)
        {
            foreach (InstanceRegistration instanceRegistration in instanceRegistrations)
            {
                container.Register(
                    Component.For(instanceRegistration.RegistrationType).Instance(instanceRegistration.Implementation));
            }
        }
    }
}
