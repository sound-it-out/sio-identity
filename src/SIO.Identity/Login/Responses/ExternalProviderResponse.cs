using System;

namespace SIO.Identity.Login.Responses
{
    public class ExternalProvider
    {
        public string DisplayName { get; }
        public string Scheme { get; }

        public ExternalProvider(string displayName, string scheme)
        {
            DisplayName = displayName;
            Scheme = scheme;
        }
    }
}
