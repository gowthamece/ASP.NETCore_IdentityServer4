using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.NCache.Options;
using IdentityServer4.NCache.Services;
using IdentityServer4.NCache.Stores;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Reflection;

namespace IdentityServer4Sample
{
    public class StartupEFCore
    {
        public StartupEFCore(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private readonly IConfiguration _configuration;
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            // migration assembly required as DbContext's are in a different assembly
            var migrationsAssembly = typeof(StartupEFCore).GetTypeInfo().Assembly.GetName().Name;
            string connectionString = _configuration.GetConnectionString("localdb");

            var builder = services.AddIdentityServer()
                .AddTestUsers(IdentityConfiguration.Users)
                .AddNCacheCaching(options =>
                {
                    options.CacheId = _configuration["CacheId"];

                    var serverList = _configuration["Servers"].Split(',')
                                            .Select(x => x.Trim())
                                            .ToList()
                                            .Select(y =>
                                                new NCacheServerInfo(y, 9800))
                                            .ToList();
                    options.ConnectionOptions = new NCacheConnectionOptions
                    {
                        ServerList = serverList
                    };

                    options.DurationOfBreakInSeconds = 120;
                })
                .AddOperationalStoreNotification<
                    EFCoreTestOperationalStoreNotification>()
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext =
                        b => b.UseSqlServer(connectionString,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext =
                        b => b.UseSqlServer(connectionString,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                    options.EnableTokenCleanup = true;
                    options.TokenCleanupInterval = 60;

                })
                .AddClientStoreCache<ClientStore>()
                .AddResourceStoreCache<ResourceStore>()
                .AddNCacheCorsPolicyCache<CorsPolicyService>()
                .AddNCachePersistedGrantStoreCache<PersistedGrantStore>(options =>
                {
                    options.CacheId = _configuration["CacheId"];

                    var serverList = _configuration["Servers"].Split(',')
                                            .Select(x => x.Trim())
                                            .ToList()
                                            .Select(y =>
                                                new NCacheServerInfo(y, 9800))
                                            .ToList();
                    options.ConnectionOptions = new NCacheConnectionOptions
                    {
                        ServerList = serverList
                    };

                    options.DurationOfBreakInSeconds = 120;
                })
                .AddNCacheProfileServiceCache<TestUserProfileService>(options =>
                {
                    options.Expiration = TimeSpan.FromMinutes(10);
                    options.KeyPrefix = "NCache-";
                    options.KeySelector =
                        (context) => context.Subject.Claims.First(
                                                        _ => _.Type == "sub").Value;
                    options.ShouldCache = (context) => true;
                });

            builder.AddDeveloperSigningCredential();

      
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                InitializeDatabase(app);
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope =
                    app.ApplicationServices
                        .GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope
                    .ServiceProvider
                        .GetRequiredService<PersistedGrantDbContext>()
                        .Database.Migrate();

                var context =
                    serviceScope.ServiceProvider
                        .GetRequiredService<ConfigurationDbContext>();

                context.Database.Migrate();
                if (!context.Clients.Any())
                {
                    foreach (var client in Config.Clients)
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.Ids)
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var resource in Config.Apis)
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}
