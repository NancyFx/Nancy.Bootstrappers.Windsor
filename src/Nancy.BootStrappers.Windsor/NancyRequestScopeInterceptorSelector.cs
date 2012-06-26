namespace Nancy.Bootstrappers.Windsor
{
    using System.Linq;
    using Castle.Core;
    using Castle.MicroKernel.Proxy;

    public class NancyRequestScopeInterceptorSelector : IModelInterceptorsSelector
    {
        /// <summary>
        /// Determine whatever the specified has interceptors.
        /// The selector should only return true from this method if it has determined that is
        /// a model that it would likely add interceptors to.
        /// </summary>
        /// <param name="model">The model</param>
        /// <returns>Whatever this selector is likely to add interceptors to the specified model</returns>
        public bool HasInterceptors(ComponentModel model)
        {
            return model.Implementation.GetInterfaces().Any(x => x == typeof(INancyEngine));
        }

        /// <summary>
        /// Select the appropriate interceptor references.
        /// The interceptor references aren't necessarily registered in the model.Intereceptors
        /// </summary>
        /// <param name="model">The model to select the interceptors for</param><param name="interceptors">The interceptors selected by previous selectors in the pipeline or <see cref="P:Castle.Core.ComponentModel.Interceptors"/> if this is the first interceptor in the pipeline.</param>
        /// <returns>The interceptor for this model (in the current context) or a null reference</returns>
        /// <remarks>
        /// If the selector is not interested in modifying the interceptors for this model, it 
        /// should return <paramref name="interceptors"/> and the next selector in line would be executed.
        /// If the selector wants no interceptors to be used it can either return <c>null</c> or empty array.
        /// However next interceptor in line is free to override this choice.
        /// </remarks>
        public InterceptorReference[] SelectInterceptors(ComponentModel model, InterceptorReference[] interceptors)
        {
            return new[] { InterceptorReference.ForType<NancyRequestScopeInterceptor>() };
        }
    }
}