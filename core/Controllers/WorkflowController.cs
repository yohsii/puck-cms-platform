using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using puck.core.Helpers;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;
using puck.core.Abstract;
using puck.core.Constants;
using puck.core.Base;
using puck.core.Entities;
using puck.core.Models;
using StackExchange.Profiling;
using System.Threading.Tasks;
using puck.core.State;
using puck.core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using puck.core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using puck.core.Concrete;
using Microsoft.Extensions.Caching.Memory;

namespace puck.core.Controllers
{
    [Area("puck")]
    public class WorkflowController : BaseController
    {
        private static readonly object _savelck = new object();
        I_Content_Indexer indexer;
        I_Content_Searcher searcher;
        I_Log log;
        I_Puck_Repository repo;
        RoleManager<PuckRole> roleManager;
        UserManager<PuckUser> userManager;
        SignInManager<PuckUser> signInManager;
        I_Content_Service contentService;
        I_Api_Helper apiHelper;
        IHostEnvironment env;
        IConfiguration config;
        IMemoryCache cache;
        public WorkflowController(I_Api_Helper ah, I_Content_Service cs, I_Content_Indexer i, I_Content_Searcher s, I_Log l, I_Puck_Repository r, RoleManager<PuckRole> rm, UserManager<PuckUser> um, SignInManager<PuckUser> sm, IHostEnvironment env, IConfiguration config, IMemoryCache cache)
        {
            this.indexer = i;
            this.searcher = s;
            this.log = l;
            this.repo = r;
            this.roleManager = rm;
            this.userManager = um;
            this.signInManager = sm;
            this.contentService = cs;
            this.apiHelper = ah;
            this.env = env;
            this.config = config;
            this.cache = cache;
            StateHelper.SetFirstRequestUrl();
            SyncIfNecessary();
        }
        

        


    }
}
