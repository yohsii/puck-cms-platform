using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using puck.core.Entities;
using puck.core.Extensions;
using Microsoft.AspNetCore.Http;
using StackExchange.Profiling.Storage;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Middleware;
using Microsoft.Extensions.FileProviders;
using puck.core.ImageSharp.WebProcessors;
using puck.core.Concrete;
using puck.core.Abstract;
using puckweb.Data.Contexts;
using puckweb.Data.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using SixLabors.ImageSharp.Web.Providers.Azure;

namespace puckweb
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }
        IHostEnvironment Env { get; }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache(x => x.SizeLimit = null);

            services.AddResponseCaching();
            services.AddSession();
            services.AddControllersWithViews()
                .AddApplicationPart(typeof(puck.core.Controllers.BaseController).Assembly)
                .AddControllersAsServices()
                .AddRazorRuntimeCompilation()
                .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
            services.AddRazorPages();
            services.AddHttpContextAccessor();

            if (Configuration.GetValue<bool?>("UseSQLServer") ?? false)
                services.AddPuckServices<User, Role, DbContextSQLServer>(Env, Configuration, ServiceLifetime.Scoped);
            else if (Configuration.GetValue<bool?>("UsePostgreSQL") ?? false)
                services.AddPuckServices<User, Role, DbContextPostgreSQL>(Env, Configuration, ServiceLifetime.Scoped);
            else if (Configuration.GetValue<bool?>("UseMySQL") ?? false)
                services.AddPuckServices<User, Role, DbContextMySQL>(Env, Configuration, ServiceLifetime.Scoped);
            else if (Configuration.GetValue<bool?>("UseSQLite") ?? false)
                services.AddPuckServices<User, Role, DbContextSQLite>(Env, Configuration, ServiceLifetime.Scoped);

            PhysicalFileSystemProvider PhysicalProviderFactory(IServiceProvider provider)
            {
                var env = provider.GetRequiredService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
                env.WebRootFileProvider = new PhysicalFileProvider(env.WebRootPath);
                var p = new PhysicalFileSystemProvider(
                    env,
                    provider.GetRequiredService<FormatUtilities>())
                {
                    Match = context =>
                    {
                        return context.Request.Path.StartsWithSegments("/media");
                    }
                };

                return p;
            }

            AzureBlobStorageImageProvider AzureProviderFactory(IServiceProvider provider)
            {
                var containerName = provider.GetService<IConfiguration>().GetValue<string>("AzureImageTransformer_ContainerName");
                var p = new AzureBlobStorageImageProvider(
                    provider.GetRequiredService<IOptions<AzureBlobStorageImageProviderOptions>>(),
                    provider.GetRequiredService<FormatUtilities>())
                {
                    Match = context =>
                    {
                        return context.Request.Path.StartsWithSegments($"/{containerName}");
                    }
                };

                return p;
            }
            if (!string.IsNullOrEmpty(Configuration.GetValue<string>("AzureBlobStorageConnectionString"))
                && !string.IsNullOrEmpty(Configuration.GetValue<string>("AzureImageTransformer_ContainerName")))
            {
                services.AddImageSharp()
                    .SetRequestParser<QueryCollectionRequestParser>()
                    .SetCache(provider =>
                    {
                        return new PhysicalFileSystemCache(
                            provider.GetRequiredService<IOptions<PhysicalFileSystemCacheOptions>>(),
                            provider.GetRequiredService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>(),
                            provider.GetRequiredService<IOptions<ImageSharpMiddlewareOptions>>(),
                            provider.GetRequiredService<FormatUtilities>());
                    })
                    .SetCacheHash<CacheHash>()
                    .AddProvider(AzureProviderFactory)
                    .Configure<AzureBlobStorageImageProviderOptions>(options =>
                    {
                        options.BlobContainers.Add(new AzureBlobContainerClientOptions
                        {
                            ConnectionString = Configuration.GetValue<string>("AzureBlobStorageConnectionString"),
                            ContainerName = Configuration.GetValue<string>("AzureImageTransformer_ContainerName")
                        });
                    })
                    .AddProvider(PhysicalProviderFactory)
                    .AddProcessor<ResizeWebProcessor>()
                    .AddProcessor<CropWebProcessor>()
                    .AddProcessor<FormatWebProcessor>()
                    .AddProcessor<BackgroundColorWebProcessor>();
            }
            else
            {
                services.AddImageSharp()
                    .SetRequestParser<QueryCollectionRequestParser>()
                    .SetCache(provider =>
                    {
                        return new PhysicalFileSystemCache(
                            provider.GetRequiredService<IOptions<PhysicalFileSystemCacheOptions>>(),
                            provider.GetRequiredService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>(),
                            provider.GetRequiredService<IOptions<ImageSharpMiddlewareOptions>>(),
                            provider.GetRequiredService<FormatUtilities>());
                    })
                    .SetCacheHash<CacheHash>()
                    .AddProvider(PhysicalProviderFactory)
                    .AddProcessor<ResizeWebProcessor>()
                    .AddProcessor<CropWebProcessor>()
                    .AddProcessor<FormatWebProcessor>()
                    .AddProcessor<BackgroundColorWebProcessor>();
            }
            services.AddMiniProfiler(options => {
                // (Optional) Path to use for profiler URLs, default is /mini-profiler-resources
                options.RouteBasePath = "/profiler";

                // (Optional) Control storage
                // (default is 30 minutes in MemoryCacheStorage)
                (options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes(60);

                // (Optional) Control which SQL formatter to use, InlineFormatter is the default
                options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();

                options.EnableServerTimingHeader = true;

                options.IgnoredPaths.Add("/lib");
                options.IgnoredPaths.Add("/css");
                options.IgnoredPaths.Add("/js");
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            var displayModes = new Dictionary<string, Func<Microsoft.AspNetCore.Http.HttpContext, bool>> {
                {"iPhone",(context)=>{return context.Request.Headers.ContainsKey("User-Agent")
                    && context.Request.Headers["User-Agent"].ToString().ToLower().Contains("iphone"); } }
            };
            puck.core.Bootstrap.Ini(Configuration, env, app.ApplicationServices, httpContextAccessor, displayModes);

            if (env.IsDevelopment())
            {
                app.UseMiniProfiler();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseImageSharp();
            app.UseStaticFiles();
            app.UseSession();
            if (!env.IsDevelopment())
            {
                app.UseResponseCaching();
            }
            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapAreaControllerRoute(
                    name: "puckarea",
                    areaName: "puck",
                    pattern: "puck/{controller=Api}/{action=Index}/{id?}"
                    );
                endpoints.MapControllerRoute(
                    name: "error",
                    pattern: "/Home/Error"
                    , defaults: new { controller = "Home", action = "Error" }
                    );
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{**path}"
                    , defaults: new { controller = "Home", action = "Index" }
                    );
                endpoints.MapRazorPages();
            });

        }

    }
}
