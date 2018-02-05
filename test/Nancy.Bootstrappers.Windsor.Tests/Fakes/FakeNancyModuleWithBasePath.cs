namespace Nancy.Bootstrappers.Windsor.Tests.Fakes
{
    using System;

    public class FakeNancyModuleWithBasePath : NancyModule
    {
        public FakeNancyModuleWithBasePath() : base("/fake")
        {
            this.Delete("/", args => {
                throw new NotImplementedException();
                return 200;
            });

            this.Get("/unique", args =>
            {
                return Guid.NewGuid().ToString();
            });

            this.Get("/route/with/some/parts", args => {
                return "FakeNancyModuleWithBasePath";
            });

            this.Get("/should/have/conflicting/route/defined", args => {
                return "FakeNancyModuleWithBasePath";
            });

            this.Get("/child/{value}", args => {
                throw new NotImplementedException();
                return 200;
            });

            this.Get("/child/route/{value}", args => {
                return "test";
            });

            this.Get("/", args => {
                throw new NotImplementedException();
                return 200;
            });

            this.Get("/foo/{value}/bar/{capture}", args => {
                return string.Concat(args.value, " ", args.capture);
                return 200;
            });

            this.Post("/", args => {
                return "Action result";
            });

            this.Put("/", args => {
                throw new NotImplementedException();
                return 200;
            });
        }
    }
}