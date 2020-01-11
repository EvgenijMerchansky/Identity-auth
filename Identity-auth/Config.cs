using Identity_auth.Configurations;
using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Identity_auth
{
    public class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources() =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId()
            };

        public static IEnumerable<ApiResource> GetApis()
        {
            IConfiguration configSections = Startup.StaticConfig;
            IdentityServerValuesConfiguration identityServerValuesConfiguration = new IdentityServerValuesConfiguration();
            configSections.Bind("IdentityServerValuesConfiguration", identityServerValuesConfiguration);

            return new List<ApiResource> {
                new ApiResource(IdentityServerConstants.LocalApi.ScopeName),
                new ApiResource(identityServerValuesConfiguration.Resources[0]),
            };
        }


        public static IEnumerable<Client> GetClients()
        {
            IConfiguration configSections = Startup.StaticConfig;
            IdentityServerValuesConfiguration identityServerValuesConfiguration = new IdentityServerValuesConfiguration();
            configSections.Bind("IdentityServerValuesConfiguration", identityServerValuesConfiguration);

            return new List<Client> {
                new Client {
                    ClientId = identityServerValuesConfiguration.ClientId,

                    AllowedGrantTypes = GrantTypes.Implicit,

                    RedirectUris = identityServerValuesConfiguration.RedirectUris,
                    PostLogoutRedirectUris = identityServerValuesConfiguration.PostLogoutRedirectUris,
                    AllowedCorsOrigins = { identityServerValuesConfiguration.Root },

                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.LocalApi.ScopeName,
                        identityServerValuesConfiguration.Resources[0]
                    },

                    AccessTokenLifetime = 1,

                    AllowOfflineAccess = true,
                    AbsoluteRefreshTokenLifetime = 3600,
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,
                    RequireClientSecret = false
                }
            };
        }
    }
}