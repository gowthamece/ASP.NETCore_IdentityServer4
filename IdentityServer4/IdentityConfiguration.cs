using IdentityModel;
using IdentityServer4.Test;
using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer4Sample
{
    public class IdentityConfiguration
    {
        public static List<TestUser> Users = new List<TestUser>
        {
            new TestUser{SubjectId = "818727", Username = "alice", Password = "alice",
                Claims =
                {
                    new Claim(JwtClaimTypes.Name, "Gowtham"),
                    new Claim(JwtClaimTypes.GivenName, "Gowtham"),
                    new Claim(JwtClaimTypes.FamilyName, "Kumar"),
                    new Claim(JwtClaimTypes.Email, "gowtham@email.com"),
                    new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                    new Claim(JwtClaimTypes.WebSite, "https://gowthamcbe.com"),
                    new Claim(JwtClaimTypes.Address, @"{ 'street_address': '59, Coimbatore', 'locality': 'Tamil Nadu', 'postal_code': 641020, 'country': 'India' }", IdentityServer4.IdentityServerConstants.ClaimValueTypes.Json)
                }
            }
          };
    }
}
