using System;

namespace Nancy.BootStrappers.Windsor.Tests.Fakes
{
    public class FakeNancyModuleWithBasePath : LegacyNancyModule
    {
        string _id;

        public FakeNancyModuleWithBasePath() : base("/fake")
        {
            _id = Guid.NewGuid().ToString();
            Delete["/"] = x => {
                throw new NotImplementedException();
            };

            Get["/unique"] = x =>
            {
                return this._id;
            };

            Get["/route/with/some/parts"] = x => {
                return "FakeNancyModuleWithBasePath";
            };

            Get["/should/have/conflicting/route/defined"] = x => {
                return "FakeNancyModuleWithBasePath";
            };

            Get["/child/{value}"] = x => {
                throw new NotImplementedException();
            };

            Get["/child/route/{value}"] = x => {
                return "test";
            };

            Get["/"] = x => {
                throw new NotImplementedException();
            };

            Get["/foo/{value}/bar/{capture}"] = x => {
                return string.Concat(x.value, " ", x.capture);
            };

            Post["/"] = x => {
                return "Action result";
            };

            Put["/"] = x => {
                throw new NotImplementedException();
            };
        }
    }
}