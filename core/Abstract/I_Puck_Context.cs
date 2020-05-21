using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using puck.core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace puck.core.Abstract
{
    public interface I_Puck_Context
    {
        int SaveChanges();
        ChangeTracker ChangeTracker { get; }
        DatabaseFacade Database { get; }
        public DbSet<PuckMeta> PuckMeta { get; set; }
        public DbSet<PuckRevision> PuckRevision { get; set; }
        public DbSet<PuckInstruction> PuckInstruction { get; set; }
        public DbSet<PuckAudit> PuckAudit { get; set; }
        public DbSet<PuckTag> PuckTag { get; set; }
        public DbSet<PuckRedirect> PuckRedirect { get; set; }
        public DbSet<PuckWorkflowItem> PuckWorkflowItem { get; set; }
        public DbSet<PuckUser> Users { get; set; }
        public DbSet<IdentityUserClaim<string>> UserClaims {get;}
        public DbSet<IdentityUserLogin<string>> UserLogins { get; }
        public DbSet<PuckUserRole> UserRoles { get; }
        public DbSet<IdentityUserToken<string>> UserTokens { get; }

    }
}
