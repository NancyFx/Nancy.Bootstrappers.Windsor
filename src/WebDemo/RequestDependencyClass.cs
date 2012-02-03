using System;
using System.Security.Cryptography;
using System.Text;

namespace WebDemo
{
    /// <summary>
    /// A module dependency that will have a per-request lifetime scope.
    /// </summary>
    public class RequestDependencyClass : IRequestDependency
    {
        private readonly DateTime currentDateTime;

        private readonly Byte[] bigDataAllocation = new byte[1000000];

        /// <summary>
        /// Initializes a new instance of the RequestDependencyClass class.
        /// </summary>
        public RequestDependencyClass()
        {
            this.currentDateTime = DateTime.Now;
            RandomNumberGenerator.Create().GetNonZeroBytes(bigDataAllocation);
        }

        public string GetContent()
        {
            return "This is a per-request dependency, constructed on: " + this.currentDateTime.ToLongTimeString() + 
                "with random data\n\n" + Encoding.UTF8.GetString(bigDataAllocation);
        }
    }
}