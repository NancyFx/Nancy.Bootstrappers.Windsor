namespace Nancy.Bootstrappers.Windsor.Tests.Fakes
{
    using System;

    public class FakeNancyModuleWithBasePath : NancyModule
    {
        public FakeNancyModuleWithBasePath() : base("/fake")
        {
            Delete("/", args => {
                throw new NotImplementedException();
                return 200;
            });

            Get("/unique", args =>
            {
                return Guid.NewGuid().ToString();
            });

            Get("/route/with/some/parts", args => {
                return "FakeNancyModuleWithBasePath";
            });

            Get("/should/have/conflicting/route/defined", args => {
                return "FakeNancyModuleWithBasePath";
            });

            Get("/child/{value}", args => {
                throw new NotImplementedException();
                return 200;
            });

            Get("/child/route/{value}", args => {
                return "test";
            });

            Get("/", args => {
                throw new NotImplementedException();
                return 200;
            });

            Get("/foo/{value}/bar/{capture}", args => {
                return string.Concat(args.value, " ", args.capture);
                return 200;
            });

            Post("/", args => {
                return "Action result";
            });

            Put("/", args => {
                throw new NotImplementedException();
                return 200;
            });
        }
    }
}