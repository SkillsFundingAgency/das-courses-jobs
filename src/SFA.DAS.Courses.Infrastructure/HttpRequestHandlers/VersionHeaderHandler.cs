namespace SFA.DAS.Courses.Infrastructure.HttpRequestHandlers
{
    public class VersionHeaderHandler : DelegatingHandler
    {
        private readonly string _version;

        public VersionHeaderHandler(string version)
        {
            _version = version ?? throw new ArgumentNullException(nameof(version));
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("X-Version", _version);
            return base.SendAsync(request, cancellationToken);
        }
    }

}