using System.Web;
using Castle.DynamicProxy;
using Castle.MicroKernel.Lifestyle;
using Castle.Windsor;

namespace Nancy.Bootstrappers.Windsor
{
    public class NancyRequestScopeInterceptor : IInterceptor
    {
        readonly IWindsorContainer _container;

        public NancyRequestScopeInterceptor(IWindsorContainer container) 
        {
            _container = container;
        }

        public void Intercept(IInvocation invocation)
        {
            // We only intercept the HandleRequest call and only wrap it in a scope if we are not using ASP.NET
            if (invocation.Method.Name != "HandleRequest" || HttpContext.Current != null)
            { 
                invocation.Proceed();
                return;
            }
            using (_container.BeginScope())
            { 
                invocation.Proceed();
            }
        }
    }
}