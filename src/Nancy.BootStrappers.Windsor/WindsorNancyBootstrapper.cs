namespace Nancy.Bootstrappers.Windsor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Castle.Core;
    using Castle.MicroKernel.Lifestyle;
    using Castle.MicroKernel.Lifestyle.Scoped;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.Resolvers.SpecializedResolvers;
    using Castle.Facilities.TypedFactory;
    using Castle.Windsor;
    using Diagnostics;
    using Bootstrapper;

    /// <summary>
    /// Nancy bootstrapper for the Windsor container.
    /// </summary>
    public abstract class WindsorNancyBootstrapper : NancyBootstrapperBase<IWindsorContainer>
    {
        private bool modulesRegistered;

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
        public override IEnumerable<INancyModule> GetAllModules(NancyContext context)
        {
            var currentScope =
                CallContextLifetimeScope.ObtainCurrentScope();

            if (currentScope != null)
            {
                return this.ApplicationContainer.ResolveAll<INancyModule>();
            }

            using (this.ApplicationContainer.BeginScope())
            {
                return this.ApplicationContainer.ResolveAll<INancyModule>();
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

            container.AddFacility<TypedFactoryFacility>();
            container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel, true));
            container.Register(Component.For<IWindsorContainer>().Instance(container));
            container.Register(Component.For<NancyRequestScopeInterceptor>());
            container.Kernel.ProxyFactory.AddInterceptorSelector(new NancyRequestScopeInterceptorSelector());

            return container;
        }

        /// <summary>
        /// Gets all registered application registration tasks
        /// </summary>
        /// <returns>An <see cref="System.Collections.Generic.IEnumerable{T}"/> instance containing <see cref="IRegistrations"/> instances.</returns>
        protected override IEnumerable<IRegistrations> GetRegistrationTasks()
        {
            return this.ApplicationContainer.ResolveAll<IRegistrations>();
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
        /// Retrieves a specific <see cref="INancyModule"/> implementation - should be per-request lifetime
        /// </summary>
        /// <param name="moduleType">Module type</param>
        /// <param name="context">The current context</param>
        /// <returns>The <see cref="INancyModule"/> instance</returns>
        public override INancyModule GetModule(Type moduleType, NancyContext context)
        {
            var currentScope =
                CallContextLifetimeScope.ObtainCurrentScope();

            if (currentScope != null)
            {
                return this.ApplicationContainer.Resolve<INancyModule>(moduleType.FullName);
            }

            using (this.ApplicationContainer.BeginScope())
            {
                return this.ApplicationContainer.Resolve<INancyModule>(moduleType.FullName);
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

            var components = moduleRegistrationTypes.Select(r => Component.For(typeof(INancyModule))
                .ImplementedBy(r.ModuleType).Named(r.ModuleType.FullName).LifeStyle.Scoped<NancyPerWebRequestScopeAccessor>())
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
        ///   Register the default implementations of internally used types into the container as singletons
        /// </summary>
        /// <param name="container"> Container to register into </param>
        /// <param name="typeRegistrations"> Type registrations to register </param>
        protected override void RegisterTypes(IWindsorContainer container, IEnumerable<TypeRegistration> typeRegistrations)
        {
            foreach (var typeRegistration in typeRegistrations)
            {
                RegisterNewOrAddService(container, typeRegistration.RegistrationType, typeRegistration.ImplementationType, typeRegistration.Lifetime);
            }
        }

        /// <summary>
        ///   Register the various collections into the container as singletons to later be resolved by IEnumerable{Type} constructor dependencies.
        /// </summary>
        /// <param name="container"> Container to register into </param>
        /// <param name="collectionTypeRegistrations"> Collection type registrations to register </param>
        protected override void RegisterCollectionTypes(IWindsorContainer container, IEnumerable<CollectionTypeRegistration> collectionTypeRegistrations)
        {
            foreach (var typeRegistration in collectionTypeRegistrations)
            {
                foreach (var implementationType in typeRegistration.ImplementationTypes)
                {
                    RegisterNewOrAddService(container, typeRegistration.RegistrationType, implementationType, typeRegistration.Lifetime);
                }
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

        private static void RegisterNewOrAddService(IWindsorContainer container, Type registrationType, Type implementationType, Lifetime lifetime)
        {
            var handler = container.Kernel.GetHandler(implementationType);
            if (handler != null)
            {
                handler.ComponentModel.AddService(registrationType);
                return;
            }

            var lifeStyle = LifestyleType.Singleton;

            switch (lifetime)
            {
                case Lifetime.Transient:
                    container.Register(
                        Component.For(implementationType, registrationType)
                            .LifestyleTransient()
                            .ImplementedBy(implementationType));
                    break;
                case Lifetime.Singleton:
                    container.Register(
                        Component.For(implementationType, registrationType)
                            .LifestyleSingleton()
                            .ImplementedBy(implementationType));
                            break;
                case Lifetime.PerRequest:
                    container.Register(
                        Component.For(implementationType, registrationType)
                            .LifestyleScoped(typeof(NancyPerWebRequestScopeAccessor))
                            .ImplementedBy(implementationType));
                    break;
                default:
                    throw new ArgumentOutOfRangeException("lifetime");
            }
        }
    }
}
