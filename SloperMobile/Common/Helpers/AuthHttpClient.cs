using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SloperMobile.Common.Helpers;

namespace SloperMobile
{
    public class AuthenticatedHttpImageClientHandler : HttpClientHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("Authorization", $"Bearer {Settings.AccessToken}");
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
