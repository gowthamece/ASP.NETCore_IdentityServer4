using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer4Sample.Configuration
{
    public static class InMemoryConfig
    {
        public static IEnumerable<IdentityResource> GetIdentityResources() =>
      new List<IdentityResource>
      {
          new IdentityResources.OpenId(),
          new IdentityResources.Profile()
      };

        public static List<TestUser> GetUsers() =>
    new List<TestUser>
    {
      new TestUser
      {
          
          SubjectId = "a9ea0f25-b964-409f-bcce-c923266249b4",
          Username = "Gowtham",
          Password = "GowthamPassword",
          Claims = new List<Claim>
          {
              new Claim("given_name", "Gowtham"),
              new Claim("family_name", "Kumar")
          }
      },
      new TestUser
      {
          SubjectId = "c95ddb8c-79ec-488a-a485-fe57a1462340",
          Username = "Shaaniya",
          Password = "ShaaniyaPassword",
          Claims = new List<Claim>
          {
              new Claim("given_name", "Shaaniya"),
              new Claim("family_name", "Gowtham")
          }
      }
    };
        public static IEnumerable<Client> GetClients() =>
    new List<Client>
    {
       new Client
       {
            ClientId = "identityClientid",
            ClientSecrets = new [] { new Secret("identityClientsecret".Sha512()) },
            AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
            AllowedScopes = { IdentityServerConstants.StandardScopes.OpenId }
        }
    };
    }
}
