//using System;
//using System.Collections.Generic;
//using System.Data.Entity;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using System.Web;
//using Microsoft.AspNet.Identity;
//using Microsoft.AspNet.Identity.EntityFramework;
//using Microsoft.AspNet.Identity.Owin;
//using Microsoft.Owin;
//using Microsoft.Owin.Security;
//using puck.core.Entities;

//namespace puck.core.Identity
//{
//    public class EmailService : IIdentityMessageService
//    {
//        public Task SendAsync(IdentityMessage message)
//        {
//            // Plug in your email service here to send an email.
//            return Task.FromResult(0);
//        }
//    }

//    public class SmsService : IIdentityMessageService
//    {
//        public Task SendAsync(IdentityMessage message)
//        {
//            // Plug in your SMS service here to send a text message.
//            return Task.FromResult(0);
//        }
//    }

//    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
//    public class PuckUserManager : UserManager<PuckUser>
//    {
//        public PuckUserManager(IUserStore<PuckUser> store)
//            : base(store)
//        {
//        }

//        public static PuckUserManager Create(IdentityFactoryOptions<PuckUserManager> options, IOwinContext context) 
//        {
//            var manager = new PuckUserManager(new UserStore<PuckUser>(context.Get<PuckContext>()));
//            // Configure validation logic for usernames
//            manager.UserValidator = new UserValidator<PuckUser>(manager)
//            {
//                AllowOnlyAlphanumericUserNames = false,
//                RequireUniqueEmail = true
//            };

//            // Configure validation logic for passwords
//            manager.PasswordValidator = new PasswordValidator
//            {
//                RequiredLength = 6,
//                RequireNonLetterOrDigit = true,
//                RequireDigit = true,
//                RequireLowercase = true,
//                RequireUppercase = true,
//            };

//            // Configure user lockout defaults
//            manager.UserLockoutEnabledByDefault = true;
//            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
//            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

//            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
//            // You can write your own provider and plug it in here.
//            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<PuckUser>
//            {
//                MessageFormat = "Your security code is {0}"
//            });
//            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<PuckUser>
//            {
//                Subject = "Security Code",
//                BodyFormat = "Your security code is {0}"
//            });
//            manager.EmailService = new EmailService();
//            manager.SmsService = new SmsService();
//            var dataProtectionProvider = options.DataProtectionProvider;
//            if (dataProtectionProvider != null)
//            {
//                manager.UserTokenProvider = 
//                    new DataProtectorTokenProvider<PuckUser>(dataProtectionProvider.Create("ASP.NET Identity"));
//            }
//            return manager;
//        }
//    }

//    public class PuckRoleManager : RoleManager<IdentityRole>
//    {
//        public PuckRoleManager(IRoleStore<IdentityRole, string> store) : base(store)
//        {
//        }
//        public static PuckRoleManager Create(IdentityFactoryOptions<PuckRoleManager> options, IOwinContext context)
//        {
//            var roleStore = new RoleStore<IdentityRole>(context.Get<PuckContext>());
//            return new PuckRoleManager(roleStore);
//        }
//    }

//    // Configure the application sign-in manager which is used in this application.
//    public class PuckSignInManager : SignInManager<PuckUser, string>
//    {
//        public PuckSignInManager(PuckUserManager userManager, IAuthenticationManager authenticationManager)
//            : base(userManager, authenticationManager)
//        {
//        }

//        public override Task<ClaimsIdentity> CreateUserIdentityAsync(PuckUser user)
//        {
//            return user.GenerateUserIdentityAsync((PuckUserManager)UserManager);
//        }

//        public static PuckSignInManager Create(IdentityFactoryOptions<PuckSignInManager> options, IOwinContext context)
//        {
//            return new PuckSignInManager(context.GetUserManager<PuckUserManager>(), context.Authentication);
//        }
//    }
//}
