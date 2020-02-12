using HMACAuthentication.Authentication;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace QueueService.Api.Authentication
{
    public class HmacSecretLookup : ISecretLookup
    {
        private readonly AppSettings appSettings;

        public HmacSecretLookup(IOptions<AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        public Task<string> LookupAsync(string id)
        {
            if (id == appSettings.HmacId)
                return Task.FromResult(appSettings.HmacSecret);
            else
                return Task.FromResult<string>(null);
        }
    }
}
