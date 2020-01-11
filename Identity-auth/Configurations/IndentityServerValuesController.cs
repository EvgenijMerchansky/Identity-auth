using System.Collections.Generic;

namespace Identity_auth.Configurations
{
    public class IdentityServerValuesConfiguration
    {
        public string ClientId { get; set; }

        public List<string> RedirectUris { get; set; }

        public List<string> Resources { get; set; }

        public List<string> PostLogoutRedirectUris { get; set; }

        public string Root { get; set; }
    }
}