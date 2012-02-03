using System;
using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.Lifestyle.Scoped;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Nancy.Bootstrapper;
using Nancy.Routing;

namespace Nancy.Bootstrappers.Windsor
{

    /// <summary>
    /// This does not create a child container because in Windsor this leads to memory leaks.  Instead be sure to use
    /// PerWebRequest lifestyle, which means it must be hosted in an ASP.NET application.
    /// </summary>
    public abstract class WindsorNancyBootstrapper : NancyBootstrapperBase<IWindsorContainer>
    {
        bool _modulesRegistered;

        protected override NancyInternalConfiguration InternalConfiguration
        {
            get
            {
                return NancyInternalConfiguration.WithOverrides(c =>
                {
                    c.ModuleKeyGenerator = typeof(WindsorModuleKeyGenerator);
                });
            }
        }

        protected override IEnumerable<IStartup> GetStartupTasks()
        {
            return this.ApplicationContainer.ResolveAll<IStartup>();
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
            if (this.ApplicationContainer != null)
            { 
                return this.ApplicationContainer;
            }
            var container = new WindsorContainer();
            container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel, true));
            container.Register(Component.For<IWindsorContainer>().Instance(container));
            container.Register(Component.For<NancyRequestScopeInterceptor>());
            container.Kernel.ProxyFactory.AddInterceptorSelector(new NancyRequestScopeInterceptorSelector());
            return container;
        }

        protected override void RegisterModules(IWindsorContainer container, IEnumerable<ModuleRegistration> moduleRegistrationTypes)
        {
            if (_modulesRegistered) return;
            _modulesRegistered = true;
            var components = moduleRegistrationTypes.Select(r => Component.For(typeof (NancyModule))
                .ImplementedBy(r.ModuleType).Named(r.ModuleKey).LifeStyle.Scoped<NancyPerWebRequestScopeAccessor>())
                .Cast<IRegistration>().ToArray();
            this.ApplicationContainer.Register(components);
        }

        public override IEnumerable<NancyModule> GetAllModules(NancyContext context)
        {
            var currentScope = CallContextLifetimeScope.ObtainCurrentScope();
            if (currentScope != null)
            { 
                return this.ApplicationContainer.ResolveAll<NancyModule>();
            }
            using (this.ApplicationContainer.BeginScope())
            {
                return this.ApplicationContainer.ResolveAll<NancyModule>();
            }
        }

        public override NancyModule GetModuleByKey(string moduleKey, NancyContext context)
        {
            var currentScope = CallContextLifetimeScope.ObtainCurrentScope();
            if (currentScope != null)
            { 
                return this.ApplicationContainer.Resolve<NancyModule>(moduleKey);
            }
            using (this.ApplicationContainer.BeginScope())
            {
                return this.ApplicationContainer.Resolve<NancyModule>(moduleKey);
            }
        }

        protected override void RegisterBootstrapperTypes(IWindsorContainer applicationContainer)
        {
            applicationContainer.Register(Component.For<INancyModuleCatalog>().Instance(this));
        }

        protected override void RegisterTypes(IWindsorContainer container, IEnumerable<TypeRegistration> typeRegistrations)
        {
            var components = typeRegistrations.Select(r => Component.For(r.RegistrationType)
                .ImplementedBy(r.ImplementationType)).Cast<IRegistration>().ToArray();
            container.Register(components);
            container.Register(Component.For<Func<IRouteCache>>()
                .UsingFactoryMethod(ctx => (Func<IRouteCache>) (ctx.Resolve<IRouteCache>)));
        }

        protected override void RegisterCollectionTypes(IWindsorContainer container, IEnumerable<CollectionTypeRegistration> collectionTypeRegistrations)
        {
            foreach (CollectionTypeRegistration collectionTypeRegistration in collectionTypeRegistrations)
            {
                foreach (Type implementationType in collectionTypeRegistration.ImplementationTypes)
                { 
                    container.Register(Component.For(collectionTypeRegistration.RegistrationType)
                        .ImplementedBy(implementationType));
                }
            }
        }

        protected override void RegisterInstances(IWindsorContainer container, IEnumerable<InstanceRegistration> instanceRegistrations)
        {
            foreach (InstanceRegistration instanceRegistration in instanceRegistrations)
            {
                container.Register(Component.For(instanceRegistration.RegistrationType)
                    .Instance(instanceRegistration.Implementation));
            }
        }
    }
}
