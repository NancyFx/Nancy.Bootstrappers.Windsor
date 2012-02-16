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
    /// Nancy bootstrapper for the Windsor container.
    /// </summary>
    public abstract class WindsorNancyBootstrapper : NancyBootstrapperBase<IWindsorContainer>
    {
        private bool modulesRegistered;
        private IEnumerable<TypeRegistration> typeRegistrations;

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

        protected override IModuleKeyGenerator GetModuleKeyGenerator()
        {
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
            if (this.modulesRegistered)
            {
                return;
            }

            this.modulesRegistered = true;

            var components = moduleRegistrationTypes.Select(r => Component.For(typeof (NancyModule))
                .ImplementedBy(r.ModuleType).Named(r.ModuleKey).LifeStyle.Scoped<NancyPerWebRequestScopeAccessor>())
                .Cast<IRegistration>().ToArray();

            this.ApplicationContainer.Register(components);
        }

        /// <summary>
        /// Get all NancyModule implementation instances
        /// </summary>
        /// <param name="context">The current context</param>
        /// <returns>An <see cref="IEnumerable{T}"/> instance containing <see cref="NancyModule"/> instances.</returns>
        public override IEnumerable<NancyModule> GetAllModules(NancyContext context)
        {
            var currentScope = 
                CallContextLifetimeScope.ObtainCurrentScope();

            if (currentScope != null)
            { 
                return this.ApplicationContainer.ResolveAll<NancyModule>();
            }

            using (this.ApplicationContainer.BeginScope())
            {
                return this.ApplicationContainer.ResolveAll<NancyModule>();
            }
        }

        /// <summary>
        /// Retrieves a specific <see cref="NancyModule"/> implementation based on its key
        /// </summary>
        /// <param name="moduleKey">Module key</param>
        /// <param name="context">The current context</param>
        /// <returns>The <see cref="NancyModule"/> instance that was retrived by the <paramref name="moduleKey"/> parameter.</returns>
        public override NancyModule GetModuleByKey(string moduleKey, NancyContext context)
        {
            var currentScope = 
                CallContextLifetimeScope.ObtainCurrentScope();

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
            this.typeRegistrations = typeRegistrations;
            
            container.Register(Component.For<Func<IRouteCache>>()
                .UsingFactoryMethod(ctx => (Func<IRouteCache>) (ctx.Resolve<IRouteCache>)));
        }
        
        protected override void RegisterCollectionTypes(IWindsorContainer container, IEnumerable<CollectionTypeRegistration> collectionTypeRegistrations)
        {
            var implementationTypes = collectionTypeRegistrations
                .SelectMany(x => x.ImplementationTypes)
                .Union(this.typeRegistrations.Select(x => x.ImplementationType))
                .Distinct();

            foreach (var implementationType in implementationTypes)
            {
                var servicesFromTypes = collectionTypeRegistrations
                    .Where(x => x.ImplementationTypes.Contains(implementationType))
                    .Select(x => x.RegistrationType);

                var servicesFromCollectionTypes = this.typeRegistrations
                    .Where(x => x.ImplementationType == implementationType)
                    .Select(x => x.RegistrationType);

               container.Register(Component.For(servicesFromCollectionTypes.Union(servicesFromTypes))
                    .ImplementedBy(implementationType).LifeStyle.Singleton);
            }
        }

        protected override void RegisterInstances(IWindsorContainer container, IEnumerable<InstanceRegistration> instanceRegistrations)
        {
            foreach (var instanceRegistration in instanceRegistrations)
            {
                container.Register(Component.For(instanceRegistration.RegistrationType)
                    .Instance(instanceRegistration.Implementation));
            }
        }
    }
}
