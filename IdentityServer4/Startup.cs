using IdentityServer4.NCache.Options;
using IdentityServer4.Test;
using IdentityServer4Sample.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.NCache.Stores;
using IdentityServer4.NCache.Services;

namespace IdentityServer4
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //    services.AddIdentityServer()
            //        .AddInMemoryIdentityResources(InMemoryConfig.GetIdentityResources())
            //.AddTestUsers(InMemoryConfig.GetUsers())
            //.AddInMemoryClients(InMemoryConfig.GetClients())
            //.AddDeveloperSigningCredential(); 

            services.AddIdentityServer()
           .AddTestUsers(InMemoryConfig.GetUsers())
           .AddNCacheConfigurationStore(options =>
           {
               options.CacheId = Configuration["NCacheConfiguration:CacheId"];

               var serverList = Configuration["NCacheConfiguration:Servers"].Split(',')
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
            .AddClientStoreCache<ClientStore>()
                .AddResourceStoreCache<ResourceStore>()
                .AddNCacheCorsPolicyCache<CorsPolicyService>();
           
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseRouting();
            app.UseIdentityServer();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
