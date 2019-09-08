namespace puck.core.Migrations
{
    using puck.core.Constants;
    using puck.core.Entities;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;

    //public class Configuration : DbMigrationsConfiguration<puck.core.Entities.PuckContext>
    //{
    //    public Configuration()
    //    {
    //        AutomaticMigrationsEnabled = true;
    //    }

    //    protected override void Seed(puck.core.Entities.PuckContext context)
    //    {
    //        var userManager = new PuckUserManager(new UserStore<PuckUser>(context));
    //        var roleManager = new PuckRoleManager(new RoleStore<IdentityRole>(context));

    //        var roles = new List<string> {PuckRoles.Cache,PuckRoles.Create,PuckRoles.Delete,PuckRoles.Domain,PuckRoles.Edit,PuckRoles.Localisation
    //        ,PuckRoles.Move,PuckRoles.Notify,PuckRoles.Publish,PuckRoles.Puck,PuckRoles.Revert,PuckRoles.Settings,PuckRoles.Sort,PuckRoles.Tasks
    //        ,PuckRoles.Unpublish,PuckRoles.Users,PuckRoles.Republish,PuckRoles.Copy,PuckRoles.ChangeType,PuckRoles.TimedPublish,PuckRoles.Audit};

    //        foreach (var roleName in roles) {
    //            if (!roleManager.RoleExists(roleName)) {
    //                var role = new IdentityRole();
    //                role.Name = roleName;
    //                roleManager.Create(role);
    //            }
    //        }
    //        var adminEmail = ConfigurationManager.AppSettings["InitialUserEmail"];
    //        var adminPassword = ConfigurationManager.AppSettings["InitialUserPassword"];
    //        if (!string.IsNullOrEmpty(adminEmail))
    //        {
    //            var admin = userManager.FindByEmail(adminEmail);
    //            if (admin == null)
    //            {
    //                admin = new PuckUser { Email = adminEmail, UserName = adminEmail };
    //                var result = userManager.Create(admin, adminPassword);

    //            }
    //            //userManager.AddPassword(admin.Id, adminPassword);
    //            foreach (var roleName in roles)
    //            {
    //                if (!userManager.IsInRole(admin.Id, roleName))
    //                    userManager.AddToRole(admin.Id, roleName);
    //            }
    //        }

    //        //  This method will be called after migrating to the latest version.

    //        //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
    //        //  to avoid creating duplicate seed data. E.g.
    //        //
    //        //    context.People.AddOrUpdate(
    //        //      p => p.FullName,
    //        //      new Person { FullName = "Andrew Peters" },
    //        //      new Person { FullName = "Brice Lambson" },
    //        //      new Person { FullName = "Rowan Miller" }
    //        //    );
    //        //
    //    }
    //}
}
