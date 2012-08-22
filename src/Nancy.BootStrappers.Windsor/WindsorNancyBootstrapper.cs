namespace Nancy.Bootstrappers.Windsor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.MicroKernel.Lifestyle;
    using Castle.MicroKernel.Lifestyle.Scoped;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.Resolvers.SpecializedResolvers;
    using Castle.Windsor;
    using Diagnostics;
    using Nancy.Bootstrapper;
    using Nancy.Routing;

    /// <summary>
    /// Nancy bootstrapper for the Windsor container.
    /// </summary>
    public abstract class WindsorNancyBootstrapper : NancyBootstrapperBase<IWindsorContainer>
    {
        private bool modulesRegistered;
        private IEnumerable<TypeRegistration> typeRegistrations;

        /// <summary>
        /// Gets the diagnostics for intialisation
        /// </summary>
        /// <returns>IDiagnostics implementation</returns>
        protected override IDiagnostics GetDiagnostics()
        {
            return this.ApplicationContainer.Resolve<IDiagnostics>();
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
        /// Gets the application level container
        /// </summary>
        /// <returns>Container instance</returns>
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

        /// <summary>
        /// Gets all registered application registration tasks
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> instance containing <see cref="IApplicationRegistrations"/> instances.</returns>
        protected override IEnumerable<IApplicationRegistrations> GetApplicationRegistrationTasks()
        {
            return this.ApplicationContainer.ResolveAll<IApplicationRegistrations>();
        }

        /// <summary>
        /// Gets all registered application startup tasks
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> instance containing <see cref="IApplicationStartup"/> instances.</returns>
        protected override IEnumerable<IApplicationStartup> GetApplicationStartupTasks()
        {
            return this.ApplicationContainer.ResolveAll<IApplicationStartup>();
        }

        /// <summary>
        /// Resolve INancyEngine
        /// </summary>
        /// <returns>INancyEngine implementation</returns>
        protected override INancyEngine GetEngineInternal()
        {
            return this.ApplicationContainer.Resolve<INancyEngine>();
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

        /// <summary>
        /// Get the moduleKey generator
        /// </summary>
        /// <returns>IModuleKeyGenerator instance</returns>
        protected override IModuleKeyGenerator GetModuleKeyGenerator()
        {
            return this.ApplicationContainer.Resolve<IModuleKeyGenerator>();
        }

        /// <summary>
        /// Nancy internal configuration
        /// </summary>
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

        /// <summary>
        /// Register the given module types into the container
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="moduleRegistrationTypes">NancyModule types</param>
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
        /// Register the bootstrapper's implemented types into the container.
        /// This is necessary so a user can pass in a populated container but not have
        /// to take the responsibility of registering things like INancyModuleCatalog manually.
        /// </summary>
        /// <param name="applicationContainer">Application container to register into</param>
        protected override void RegisterBootstrapperTypes(IWindsorContainer applicationContainer)
        {
            applicationContainer.Register(Component.For<INancyModuleCatalog>().Instance(this));
        }

        /// <summary>
        /// Register the default implementations of internally used types into the container as singletons
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="typeRegistrations">Type registrations to register</param>
        protected override void RegisterTypes(IWindsorContainer container, IEnumerable<TypeRegistration> typeRegistrations)
        {
            this.typeRegistrations = typeRegistrations;
            
            container.Register(Component.For<Func<IRouteCache>>()
                .UsingFactoryMethod(ctx => (Func<IRouteCache>) (ctx.Resolve<IRouteCache>)));
        }

        /// <summary>
        /// Register the various collections into the container as singletons to later be resolved
        /// by IEnumerable{Type} constructor dependencies.
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="collectionTypeRegistrations">Collection type registrations to register</param>
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

        /// <summary>
        /// Register the given instances into the container
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="instanceRegistrations">Instance registration types</param>
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
