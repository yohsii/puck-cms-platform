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
using puckweb.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using puck.core.Entities;
using puck.core.Abstract;
using puck.core.Concrete;
using puck.core.Helpers;
using puck.core.Services;
using puck.core.State;
using puck.core.Extensions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Serialization;
using System.Text.Json;
using StackExchange.Profiling.Storage;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Middleware;

namespace puckweb
{
    public class Startup
    {
        public Startup(IConfiguration configuration,IHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }
        IHostEnvironment Env { get; }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<PuckContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection"))
                ,optionsLifetime:ServiceLifetime.Transient);
            services.AddDefaultIdentity<PuckUser>(options => { options.SignIn.RequireConfirmedAccount = false;})
                .AddRoles<PuckRole>()
                .AddEntityFrameworkStores<PuckContext>();
            services.AddMemoryCache();
            services.AddResponseCaching();
            services.AddSession();
            services.AddControllersWithViews()
                .AddApplicationPart(typeof(puck.core.Controllers.BaseController).Assembly)
                .AddControllersAsServices()
                .AddRazorRuntimeCompilation()
                .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
            services.AddRazorPages();
            services.AddAuthentication().AddCookie(puck.core.Constants.Mvc.AuthenticationScheme, options=> {
                options.LoginPath = "/puck/admin/in";
                options.LogoutPath = "/puck/admin/out";
                options.AccessDeniedPath= "/puck/admin/in";
                options.ForwardAuthenticate = "Identity.Application";
            });
            
            services.AddHttpContextAccessor();
            services.AddPuckServices(Env,Configuration);

            PhysicalFileSystemProvider PhysicalProviderFactory(IServiceProvider provider)
            {
                var p = new PhysicalFileSystemProvider(
                    provider.GetRequiredService<Microsoft.AspNetCore.Hosting.IHostingEnvironment>(),
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

            services.AddImageSharpCore()
                .SetRequestParser<QueryCollectionRequestParser>()
                .SetCache(provider =>
                {
                    return new PhysicalFileSystemCache(
                        provider.GetRequiredService<IOptions<PhysicalFileSystemCacheOptions>>(),
                        provider.GetRequiredService<Microsoft.AspNetCore.Hosting.IHostingEnvironment>(),
                        provider.GetRequiredService<IOptions<ImageSharpMiddlewareOptions>>(),
                        provider.GetRequiredService<FormatUtilities>());
                })
                .SetCacheHash<CacheHash>()
                .AddProvider(AzureProviderFactory)
                .Configure<AzureBlobStorageImageProviderOptions>(options =>
                {
                    options.ConnectionString = Configuration.GetValue<string>("AzureBlobStorageConnectionString");
                    options.ContainerName = Configuration.GetValue<string>("AzureImageTransformer_ContainerName");
                })
                .AddProvider(PhysicalProviderFactory)
                .AddProcessor<ResizeWebProcessor>()
                .AddProcessor<FormatWebProcessor>()
                .AddProcessor<BackgroundColorWebProcessor>();

            services.AddMiniProfiler(options =>{
                // (Optional) Path to use for profiler URLs, default is /mini-profiler-resources
                //options.RouteBasePath = "/profiler";

                // (Optional) Control storage
                // (default is 30 minutes in MemoryCacheStorage)
                (options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes(60);

                // (Optional) Control which SQL formatter to use, InlineFormatter is the default
                options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();

            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            var displayModes = new Dictionary<string, Func<Microsoft.AspNetCore.Http.HttpContext,bool>> {
                {"iPhone",(context)=>{return context.Request.Headers.ContainsKey("User-Agent") 
                    && context.Request.Headers["User-Agent"].ToString().ToLower().Contains("iphone"); } }
            };
            puck.core.Bootstrap.Ini(Configuration,env,app.ApplicationServices, httpContextAccessor,displayModes);
            
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
            app.UseResponseCaching();
            app.UseRouting();
            app.UseAuthentication();
            
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapAreaControllerRoute(
                    name:"puckarea",
                    areaName:"puck",
                    pattern: "puck/{controller=Api}/{action=Index}/{id?}"
                    );
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{**path}"
                    ,defaults: new { controller = "Home", action = "Index"}
                    );
                endpoints.MapRazorPages();
            });
        }
    }
}
