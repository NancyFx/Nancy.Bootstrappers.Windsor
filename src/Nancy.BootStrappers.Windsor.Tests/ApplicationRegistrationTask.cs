namespace Nancy.Bootstrappers.Windsor.Tests
{
    using Bootstrapper;

    public class ApplicationRegistrationTask : Registrations
    {
        public ApplicationRegistrationTask()
        {
            Register<IType1>(typeof(Type));
            Register<IType2>(typeof(Type));
        }

        public interface IType1 { }
        public interface IType2 { }
        public class Type : IType1, IType2 { }
    }
}