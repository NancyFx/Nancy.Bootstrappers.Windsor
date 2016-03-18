namespace Nancy.Bootstrappers.Windsor
{
    using System.Web;
    using Castle.MicroKernel.Context;
    using Castle.MicroKernel.Lifestyle;
    using Castle.MicroKernel.Lifestyle.Scoped;

    public class NancyPerWebRequestScopeAccessor : IScopeAccessor
    {
        private readonly WebRequestScopeAccessor webScopeAccessor;
        private readonly LifetimeScopeAccessor defaultScopeAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="NancyPerWebRequestScopeAccessor"/> class.
        /// </summary>
        public NancyPerWebRequestScopeAccessor() 
        {
            this.webScopeAccessor = new WebRequestScopeAccessor();
            this.defaultScopeAccessor = new LifetimeScopeAccessor();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (HttpContext.Current == null)
            {
                defaultScopeAccessor.Dispose();
                return;
            }

            webScopeAccessor.Dispose();
        }

        /// <summary>
        /// Provides access to <see cref="T:Castle.MicroKernel.Lifestyle.Scoped.IScopeCache"/> for currently resolved component.
        /// </summary>
        /// <param name="context">Current creation context</param>
        /// <exception cref="T:System.InvalidOperationException">Thrown when scope cache could not be accessed.</exception>
        public ILifetimeScope GetScope(CreationContext context)
        {
            return HttpContext.Current == null ? defaultScopeAccessor.GetScope(context) : webScopeAccessor.GetScope(context);
        }
    }
}