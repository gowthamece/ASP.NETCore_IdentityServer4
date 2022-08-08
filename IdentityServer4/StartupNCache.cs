using IdentityServer4;
using IdentityServer4.NCache.Entities;
using IdentityServer4.NCache.Mappers;
using IdentityServer4.NCache.Options;
using IdentityServer4.NCache.Stores.Interfaces;
using IdentityServer4Sample.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Linq;

namespace IdentityServer4Sample
{
    public class StartupNCache
    {
        public StartupNCache(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private readonly IConfiguration _configuration;
        public void ConfigureServices(IServiceCollection services)
        {
            var builder = services.AddIdentityServer()
            .AddTestUsers(InMemoryConfig.GetUsers())
            /// Add NCache as a configuration store
            .AddNCacheConfigurationStore(options =>
            {
                options.CacheId = _configuration["NCacheConfiguration:CacheId"];

                var serverList = _configuration["NCacheConfiguration:Servers"].Split(',')
                            .Select(x => x.Trim())
                            .ToList()
                            .Select(y =>
                                new NCacheServerInfo(y, 9800))
                            .ToList();
                options.ConnectionOptions = new NCacheConnectionOptions
                {
                    ServerList = serverList,
                    EnableClientLogs = true,
                    LogLevel = NCacheLogLevel.Debug
                };
            })
            /// Add NCache as an operational store
               .AddNCachePersistedGrantStore(options =>
               {
                   options.CacheId = _configuration["NCacheConfiguration:CacheId"];

                   var serverList = _configuration["NCacheConfiguration:Servers"].Split(',')
                               .Select(x => x.Trim())
                               .ToList()
                               .Select(y =>
                                   new NCacheServerInfo(y, 9800))
                               .ToList();
                   options.ConnectionOptions = new NCacheConnectionOptions
                   {
                       ServerList = serverList,
                       EnableClientLogs = true,
                       LogLevel = NCacheLogLevel.Debug
                   };
               })
             .AddNCacheDeviceCodeStore(options =>
             {
                 options.CacheId = _configuration["NCacheConfiguration:CacheId"];

                 var serverList = _configuration["NCacheConfiguration:Servers"].Split(',')
                             .Select(x => x.Trim())
                             .ToList()
                             .Select(y =>
                                 new NCacheServerInfo(y, 9800))
                             .ToList();
                 options.ConnectionOptions = new NCacheConnectionOptions
                 {
                     ServerList = serverList,
                     EnableClientLogs = true,
                     LogLevel = NCacheLogLevel.Debug
                 };
             });

            //services.AddAuthentication()
            //    .AddGoogle("Google", options =>
            //    {
            //        options.SignInScheme =
            //            IdentityServerConstants.ExternalCookieAuthenticationScheme;

            //        options.ClientId = "<insert here>";
            //        options.ClientSecret = "<insert here>";
            //    })
            //    .AddOpenIdConnect("oidc", "Demo IdentityServer", options =>
            //    {
            //        options.SignInScheme =
            //            IdentityServerConstants.ExternalCookieAuthenticationScheme;
            //        options.SignOutScheme = IdentityServerConstants.SignoutScheme;
            //        options.SaveTokens = true;

            //        options.Authority = "https://demo.identityserver.io/";
            //        options.ClientId = "native.code";
            //        options.ClientSecret = "secret";
            //        options.ResponseType = "code";

            //        options.TokenValidationParameters = new TokenValidationParameters
            //        {
            //            NameClaimType = "name",
            //            RoleClaimType = "role"
            //        };
            //    });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                InitializeNCacheStore(app);
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.UseIdentityServer();
            //   app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
        private void InitializeNCacheStore(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices
                                            .GetService<IServiceScopeFactory>()
                                                .CreateScope())
            {
                var clientContext = serviceScope.ServiceProvider
                    .GetRequiredService<IConfigurationStoreRepository<Client>>();
                var clientCount = clientContext
                                   .GetMultipleByTagsAsync(
                                       new string[] { "Client" })
                                   .GetAwaiter()
                                   .GetResult()
                                   .ToList()
                                   .Count;
                if (clientCount == 0)
                {
                    var clients = InMemoryConfig.GetClients().ToList().Select(x => x.ToEntity());
                    clientContext.AddAsync(clients).Wait();
                }
            }
       }
      
    }
}
