using System.Linq;
using Castle.Core;
using Castle.MicroKernel.Proxy;

namespace Nancy.Bootstrappers.Windsor
{
    public class NancyRequestScopeInterceptorSelector : IModelInterceptorsSelector
    {
        public bool HasInterceptors(ComponentModel model)
        {
            return model.Implementation.GetInterfaces().Any(x => x == typeof(INancyEngine));
        } 

        public InterceptorReference[] SelectInterceptors(ComponentModel model, InterceptorReference[] interceptors)
        {
            return new[] { InterceptorReference.ForType<NancyRequestScopeInterceptor>() };
        }
    }
}