namespace Nancy.Bootstrappers.Windsor.Tests
{
    using System.Collections.Generic;
    using Bootstrapper;

    public class ApplicationRegistrationTask : IApplicationRegistrations
    {
        public IEnumerable<TypeRegistration> TypeRegistrations
        {
            get
            {
                return new[]
                         {
                             new TypeRegistration(typeof(IType1), typeof(Type)),
                             new TypeRegistration(typeof(IType2), typeof(Type))
                         };
            }
        }

        public IEnumerable<CollectionTypeRegistration> CollectionTypeRegistrations
        {
            get { return null; }
        }

        public IEnumerable<InstanceRegistration> InstanceRegistrations
        {
            get { return null; }
        }

        public interface IType1 { }
        public interface IType2 { }
        public class Type : IType1, IType2 { }
    }
}