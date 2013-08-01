namespace Nancy.Bootstrappers.Windsor
{
    using System;
    using Castle.Windsor;

    public class NancyModuleReleaser : IDisposable
    {
        IWindsorContainer container;
        object instance;

        public NancyModuleReleaser(object instance, IWindsorContainer container)
        {
            this.instance = instance;
            this.container = container;
        }

        public void Dispose()
        {
            container.Release(instance);
            instance = null;
            container = null;
        }
    }
}