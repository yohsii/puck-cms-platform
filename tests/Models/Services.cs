using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using puck.core.Abstract;
using puck.core.Concrete;
using puck.core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace puck.tests.Models
{
    public class Services
    {
        public I_Content_Service ContentService;
        public I_Content_Indexer Indexer;
        public I_Content_Searcher Searcher;
        public I_Puck_Repository Repo;
        public I_Log Logger;
        public I_Api_Helper ApiHelper;
        public I_Task_Dispatcher TDispatcher;
        public RoleManager<PuckRole> RoleManager;
        public UserManager<PuckUser> UserManager;
        public SqliteConnection Con;
        public DbContextOptionsBuilder<PuckContext> DbContextOptionsBuilder;
        public I_Puck_Context Context;

    }
}
