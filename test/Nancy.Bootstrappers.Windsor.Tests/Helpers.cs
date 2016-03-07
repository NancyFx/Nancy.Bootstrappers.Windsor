namespace Nancy.Bootstrappers.Windsor.Tests
{
    using System.IO;

    public static class Helpers
    {
        public static string GetContentsAsString(this Response r)
        {
            var stream = new MemoryStream();
            r.Contents.Invoke(stream);
            stream.Position = 0;
            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}