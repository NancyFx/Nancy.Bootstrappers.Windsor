namespace Nancy.Bootstrappers.Windsor
{
    using System;
    using Castle.Windsor;

    /// <summary>
    /// Disposable class that wraps an object that needs to be released from the WinsdorContainer at the end of the NancyRequest
    /// </summary>
    public class NancyModuleReleaser : IDisposable
    {
        private IWindsorContainer container;
        private object instance;

        /// <summary>
        /// Creates an instance of the NancyModuleReleaser that will wrap the object that needs to be released from the container
        /// </summary>
        /// <param name="instance">The object that has been explicitly resolved from the Container</param>
        /// <param name="container">The container the object has been resolved from</param>
        public NancyModuleReleaser(object instance, IWindsorContainer container)
        {
            this.instance = instance;
            this.container = container;
        }

        /// <summary>
        /// Released the instance from the container
        /// </summary>
        public void Dispose()
        {
            this.container.Release(instance);
            this.instance = null;
            this.container = null;
        }
    }
}