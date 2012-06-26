namespace Nancy.Bootstrappers.Windsor
{
    using System.Web;
    using Castle.DynamicProxy;
    using Castle.MicroKernel.Lifestyle;
    using Castle.Windsor;

    public class NancyRequestScopeInterceptor : IInterceptor
    {
        private readonly IWindsorContainer container;

        /// <summary>
        /// Initializes a new instance of the <see cref="NancyRequestScopeInterceptor"/> class,
        /// with the provided <paramref name="container"/>.
        /// </summary>
        /// <param name="container">The container that should be scoped.</param>
        public NancyRequestScopeInterceptor(IWindsorContainer container) 
        {
            this.container = container;
        }

        public void Intercept(IInvocation invocation)
        {
            // We only intercept the HandleRequest call and only wrap it in a scope if we are not using ASP.NET
            if (invocation.Method.Name != "HandleRequest" || HttpContext.Current != null)
            { 
                invocation.Proceed();
                return;
            }

            using (container.BeginScope())
            { 
                invocation.Proceed();
            }
        }
    }
}