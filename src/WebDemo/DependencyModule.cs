using System;
using Nancy;

namespace WebDemo
{
    public class DependencyModule : NancyModule
    {
        private readonly IApplicationDependency applicationDependency;
        private readonly IRequestDependency requestDependency;

        public DependencyModule(IApplicationDependency applicationDependency, IRequestDependency requestDependency)
        {
            this.applicationDependency = applicationDependency;
            this.requestDependency = requestDependency;

            Get["/dependency1"] = x => "Hello World " + this.applicationDependency.GetContent();

            Get["/dependency2"] = x => "Hello World " + this.requestDependency.GetContent();
        }
    }
}