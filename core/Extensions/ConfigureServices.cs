using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using puck.core.Abstract;
using puck.core.Concrete;
using puck.core.Helpers;
using puck.core.Services;
using puck.core.State;
using System;
using System.Collections.Generic;
using System.Text;

namespace puck.core.Extensions
{
    public static class ConfigureServices
    {
        public static void AddPuckServices(this IServiceCollection services,IHostEnvironment env,IConfiguration config) {
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

        }
    }
}
