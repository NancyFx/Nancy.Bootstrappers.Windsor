namespace Nancy.Bootstrappers.Windsor.Tests.Fakes
{
    public interface IFoo
    {
    }

    public class Foo : IFoo
    {
    }

    public interface IDependency
    {
        IFoo FooDependency { get; set; }
    }

    public class FakeDependency : IDependency
    {
        public IFoo FooDependency { get; set; }

        /// <summary>
        /// Initializes a new instance of the Dependency class.
        /// </summary>
        public FakeDependency(IFoo fooDependency)
        {
            FooDependency = fooDependency;
        }
    }

    public class FakeNancyModuleWithDependency : NancyModule
    {
        public IDependency Dependency { get; set; }
        public IFoo FooDependency { get; set; }

        /// <summary>
        /// Initializes a new instance of the FakeNancyModuleWithDependency class.
        /// </summary>
        public FakeNancyModuleWithDependency(IDependency dependency, IFoo foo)
        {
            Dependency = dependency;
            FooDependency = foo;
            Get("/with-dependency", args => "a-ok");
        }
    }
}
