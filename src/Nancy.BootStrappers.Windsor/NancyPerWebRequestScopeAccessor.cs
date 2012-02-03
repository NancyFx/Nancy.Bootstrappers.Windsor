using System.Web;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.Lifestyle.Scoped;

namespace Nancy.Bootstrappers.Windsor
{
    public class NancyPerWebRequestScopeAccessor : IScopeAccessor
    {
        readonly WebRequestScopeAccessor _webScopeAccessor;
        readonly LifetimeScopeAccessor _defaultScopeAccessor;

        public NancyPerWebRequestScopeAccessor() 
        {
            _webScopeAccessor = new WebRequestScopeAccessor();
            _defaultScopeAccessor = new LifetimeScopeAccessor();
        }

        public void Dispose()
        {
            if (HttpContext.Current == null)
            {
                _defaultScopeAccessor.Dispose();
                return;
            }
            _webScopeAccessor.Dispose();
        }

        public ILifetimeScope GetScope(CreationContext context)
        {
            if (HttpContext.Current == null)
            {
                return _defaultScopeAccessor.GetScope(context);
            }
            return _webScopeAccessor.GetScope(context);
        }
    }
}