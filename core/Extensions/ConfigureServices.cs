using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using puck.core.Abstract;
using puck.core.Concrete;
using puck.core.Entities;
using puck.core.Helpers;
using puck.core.Identity;
using puck.core.Services;
using puck.core.State;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace puck.core.Extensions
{
    public static class ConfigureServices
    {
        public static void AddPuckServices<TUser,TRole,TDbContext>(this IServiceCollection services,IHostEnvironment env,IConfiguration config, ServiceLifetime dbContextLifetime
            , Action<CookieAuthenticationOptions> configureCookieAuthenticationOptions = null,Action<IdentityOptions> configureIdentityOptions=null
            ,Action<IdentityBuilder> configureIdentityBuilder=null)
            where TDbContext : DbContext where TUser : IdentityUser where TRole: IdentityRole {
            PuckCache.ContentRootPath = env.ContentRootPath;
            var logger = new Logger();
            var indexerSearcher = new Content_Indexer_Searcher(logger, config,env);
            services.AddTransient<I_Puck_Repository, Puck_Repository>();
            services.AddSingleton<I_Content_Indexer>(indexerSearcher);
            services.AddSingleton<I_Content_Searcher>(indexerSearcher);
            services.AddTransient<I_Log, Logger>();
            services.AddSingleton<I_Task_Dispatcher, Dispatcher>();
            services.AddTransient<I_Api_Helper,ApiHelper>();
            services.AddTransient<I_Log_Helper,LogHelper>();
            services.AddTransient<I_Content_Service,ContentService>();
            services.AddHostedService<Dispatcher>((IServiceProvider serviceProvider) => { return serviceProvider.GetService<I_Task_Dispatcher>() as Dispatcher; });
            services.AddScoped<PuckCookieAuthenticationEvents>();

            //services.AddDefaultIdentity<PuckUser>(options => { options.SignIn.RequireConfirmedAccount = false; })
            services.AddAuthentication(o =>
            {
                o.DefaultScheme = IdentityConstants.ApplicationScheme;
                o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            }).AddIdentityCookies(o => {
                o.ApplicationCookie.Configure(x => { 
                    x.EventsType = typeof(PuckCookieAuthenticationEvents);
                    configureCookieAuthenticationOptions?.Invoke(x);
                });
            });

            var identityBuilder = services.AddIdentityCore<PuckUser>(o =>
            {
                o.Stores.MaxLengthForKeys = 128;
                o.SignIn.RequireConfirmedAccount = false;
            })
                .AddDefaultTokenProviders()
                .AddRoles<PuckRole>();

            services.AddScoped<ISecurityStampValidator, SecurityStampValidator<TUser>>();
            services.AddScoped<ISecurityStampValidator, SecurityStampValidator<PuckUser>>();

            if (config.GetValue<bool?>("UseSQLServer") ?? false)
            {
                services.AddEntityFrameworkSqlServer().AddDbContext<PuckContextSQLServer>(optionsLifetime: ServiceLifetime.Transient);
                identityBuilder.AddEntityFrameworkStores<PuckContextSQLServer>();
                services.AddTransient<I_Puck_Context>(x => x.GetService<PuckContextSQLServer>());
                //add front end db context and identity
                services.AddDbContext<TDbContext>(x => x.UseSqlServer(config.GetConnectionString("SQLServer")), optionsLifetime: dbContextLifetime);
                var identBuilder = services.AddIdentityCore<TUser>(options => { 
                    options.SignIn.RequireConfirmedAccount = false;
                    configureIdentityOptions?.Invoke(options);
                })
                    .AddRoles<TRole>()
                    .AddEntityFrameworkStores<TDbContext>();
                configureIdentityBuilder?.Invoke(identBuilder);
            }else if (config.GetValue<bool?>("UsePostgreSQL") ?? false)
            {
                services.AddEntityFrameworkNpgsql().AddDbContext<PuckContextPostgreSQL>(optionsLifetime: ServiceLifetime.Transient);
                identityBuilder.AddEntityFrameworkStores<PuckContextPostgreSQL>();
                services.AddTransient<I_Puck_Context>(x => x.GetService<PuckContextPostgreSQL>());
                //add front end db context and identity
                services.AddDbContext<TDbContext>(x => x.UseNpgsql(config.GetConnectionString("PostgreSQL")), optionsLifetime: dbContextLifetime);
                var identBuilder = services.AddIdentityCore<TUser>(options => {
                    options.SignIn.RequireConfirmedAccount = false;
                    configureIdentityOptions?.Invoke(options);
                })
                    .AddRoles<TRole>()
                    .AddEntityFrameworkStores<TDbContext>();
                configureIdentityBuilder?.Invoke(identBuilder);
            }
            else if (config.GetValue<bool?>("UseMySQL") ?? false)
            {
                services.AddEntityFrameworkMySql().AddDbContext<PuckContextMySQL>(optionsLifetime: ServiceLifetime.Transient);
                identityBuilder.AddEntityFrameworkStores<PuckContextMySQL>();
                services.AddTransient<I_Puck_Context>(x => x.GetService<PuckContextMySQL>());
                //add front end db context and identity
                services.AddDbContext<TDbContext>(x => x.UseMySql(config.GetConnectionString("MySQL"),ServerVersion.AutoDetect(config.GetConnectionString("MySQL"))), optionsLifetime: dbContextLifetime);
                var identBuilder = services.AddIdentityCore<TUser>(options => {
                    options.SignIn.RequireConfirmedAccount = false;
                    configureIdentityOptions?.Invoke(options);
                })
                    .AddRoles<TRole>()
                    .AddEntityFrameworkStores<TDbContext>();
                configureIdentityBuilder?.Invoke(identBuilder);
            }
            else if (config.GetValue<bool?>("UseSQLite") ?? false)
            {
                services.AddEntityFrameworkSqlite().AddDbContext<PuckContextSQLite>(optionsLifetime: ServiceLifetime.Transient);
                identityBuilder.AddEntityFrameworkStores<PuckContextSQLite>();
                services.AddTransient<I_Puck_Context>(x => x.GetService<PuckContextSQLite>());
                //add front end db context and identity
                services.AddDbContext<TDbContext>(x => x.UseSqlite(config.GetConnectionString("SQLite")), optionsLifetime: dbContextLifetime);
                var identBuilder = services.AddIdentityCore<TUser>(options => {
                    options.SignIn.RequireConfirmedAccount = false;
                    configureIdentityOptions?.Invoke(options);
                })
                    .AddRoles<TRole>()
                    .AddEntityFrameworkStores<TDbContext>();
                configureIdentityBuilder?.Invoke(identBuilder);
            }

            services.AddAuthentication()
                .AddCookie(puck.core.Constants.Mvc.AuthenticationScheme, options => {
                    options.LoginPath = "/puck/admin/in";
                    options.LogoutPath = "/puck/admin/out";
                    options.AccessDeniedPath = "/puck/admin/in";
                    options.ForwardAuthenticate = IdentityConstants.ApplicationScheme;
                });

            services.AddScoped<SignInManager<PuckUser>>();
            services.AddScoped<SignInManager<TUser>>();
            services.AddScoped<IUserClaimsPrincipalFactory<PuckUser>, PuckClaimsPrincipalFactory>();
        }
    }
}
