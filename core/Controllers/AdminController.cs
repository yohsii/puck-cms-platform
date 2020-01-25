using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using puck.core.Models;
using puck.core.Models.Admin;
using puck.core.Abstract;
using puck.core.Constants;
using Newtonsoft.Json;
using puck.core.Entities;
using puck.core.Filters;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using puck.core.State;
using Microsoft.Extensions.Caching.Memory;

namespace puck.core.Controllers
{
    [Area("puck")]
    public class AdminController : Controller
    {
        I_Content_Indexer indexer;
        I_Content_Searcher searcher;
        I_Log log;
        I_Puck_Repository repo;
        RoleManager<PuckRole> roleManager;
        UserManager<PuckUser> userManager;
        SignInManager<PuckUser> signInManager;
        IMemoryCache cache;
        
        public AdminController(I_Content_Indexer i, I_Content_Searcher s, I_Log l, I_Puck_Repository r,RoleManager<PuckRole> rm,UserManager<PuckUser> um, SignInManager<PuckUser> sm,IMemoryCache cache) {
            this.cache = cache;
            this.indexer = i;
            this.searcher = s;
            this.log = l;
            this.repo = r;
            this.roleManager = rm;
            this.userManager = um;
            this.signInManager = sm;
        }

        [HttpGet]
        public ActionResult ForgotPassword() {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> ForgotPassword(ForgottenPassword model) {
            if (ModelState.IsValid) {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null) {
                    //ModelState.AddModelError("", "Email doesn't exist");
                    ViewBag.SuccessMessage = $"An email will be sent to the specified address with instructions on how to reset your password, if the address matches our records";
                    return View(model);
                }
                
                var token = Guid.NewGuid().ToString();
                
                var meta = new PuckMeta {
                    Name = DBNames.PasswordResetToken
                    , Key = token
                    , Value = model.Email
                };
                repo.AddPuckMeta(meta);
                repo.SaveChanges();

                var uri = HttpContext.Request.GetUri() ?? PuckCache.FirstRequestUrl;
                var resetUrl = uri.Scheme + "://"
                    + uri.Host
                    + (uri.Port != 80 ? (":" + uri.Port) : "")
                    + $"/puck/admin/resetpassword?token={token}";
               
                puck.core.Helpers.ApiHelper.Email(
                    model.Email
                    ,"Reset your password - Puck CMS"
                    ,$"click <a href=\"{resetUrl}\">here</a> to reset your password."
                );
                
                ViewBag.SuccessMessage = $"An email will be sent to the specified address with instructions on how to reset your password, if the address matches our records";

                return View(model);
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult ResetPassword(string token) {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("in");

            var model = new ResetPassword { 
                ResetToken = token
            };

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> ResetPassword(ResetPassword model) {
            if (ModelState.IsValid) {
                var meta = repo.GetPuckMeta().Where(x => x.Name == DBNames.PasswordResetToken && x.Key == model.ResetToken).FirstOrDefault();

                if (meta == null) {
                    ModelState.AddModelError("","Password reset request not found.");
                    return View(model);
                }

                var user = await userManager.FindByEmailAsync(meta.Value);

                if (user == null)
                {
                    ModelState.AddModelError("", "User not found.");
                    return View(model);
                }

                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var result = await userManager.ResetPasswordAsync(user, token, model.Password);

                if (!result.Succeeded)
                {
                    string message = string.Join(" ", result.Errors.Select(x => x.Description));
                    ModelState.AddModelError("",message);
                    return View(model);
                }

                repo.DeletePuckMeta(meta);
                repo.SaveChanges();

                ViewBag.SuccessMessage = "Password successfully changed.";

                return View(model);
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult In() {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> In(LogIn user,string returnUrl) {
            var result = await this.signInManager.PasswordSignInAsync(user.Username, user.Password, user.PersistentCookie, false);
            if (result.Succeeded)
            {
                var puckUser = await userManager.FindByNameAsync(user.Username);
                puckUser.PuckLastLoginDate = DateTime.Now;
                await userManager.UpdateAsync(puckUser);
                if (!string.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("Index", "api", new { area = "puck" });
            }
            else
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                ViewBag.Error = "Incorrect Login Information";
                return View(user);
            }
        }

        public async Task<ActionResult> Out() {
            HttpContext.Session.Clear();
            await signInManager.SignOutAsync();
            return RedirectToAction("In");
        }

        //TODO renew ticket to stay signed in while using cms
        public ActionResult Renew() {
            return View();
        }
        private async Task<List<PuckUserViewModel>> GetUsers() {
            var model = new List<PuckUserViewModel>();
            var puckRole = await roleManager.FindByNameAsync(PuckRoles.Puck);
            var userCollection = repo.GetPuckUser().Where(x => x.Roles.Any(xx => xx.RoleId == puckRole.Id)).ToList();

            foreach (PuckUser pu in userCollection)
            {
                var puvm = new PuckUserViewModel();
                puvm.LastLoginDate = pu.PuckLastLoginDate;
                puvm.LastLoginDateString = "user has never logged in";
                if (puvm.LastLoginDate.HasValue)
                    puvm.LastLoginDateString = puvm.LastLoginDate.Value.ToString("dd/MM/yyyy hh:mm");
                puvm.UserName = pu.UserName;
                puvm.Email = pu.Email;
                puvm.FirstName = pu.PuckFirstName;
                puvm.Surname = pu.PuckSurname;
                puvm.User = pu;
                puvm.Roles = (await userManager.GetRolesAsync(pu)).ToList();
                if (!string.IsNullOrEmpty(pu.PuckStartNodeIds))
                    puvm.StartNodes = pu.PuckStartNodeIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => new PuckReference { Id = Guid.Parse(x) }).ToList();
                puvm.UserVariant = pu.PuckUserVariant;
                puvm.StartPaths = "/";
                if (puvm.StartNodes!=null && puvm.StartNodes.Any()) {
                    Guid[] startIds = puvm.StartNodes.Select(x => x.Id).ToArray();
                    var nodes = repo.GetPuckRevision().Where(x => startIds.Contains(x.Id) && x.Current).ToList().GroupBy(x=>x.Id);
                    if (nodes.Any())
                        puvm.StartPaths = string.Join(",",nodes.Select(x=>x.First().Path));
                }
                model.Add(puvm);
            }
            return model;
        }
        [Authorize(Roles = PuckRoles.Users, AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public async Task<ActionResult> Users()
        {
            var model = await GetUsers();
            return Json(model);
        }
        [Authorize(Roles =PuckRoles.Users,AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public async Task<ActionResult> Index()
        {
            var model = await GetUsers();
            return View(model);
        }

        [Authorize(Roles =PuckRoles.Users, AuthenticationSchemes =Mvc.AuthenticationScheme)]
        public async Task<ActionResult> Edit(string userName=null) {
            var model = new PuckUserViewModel();
            ViewBag.Level0Type = typeof(PuckUserViewModel);
            if (!string.IsNullOrEmpty(userName)) {
                var usr = await userManager.FindByNameAsync(userName);
                model.FirstName = usr.PuckFirstName;
                model.Surname = usr.PuckSurname;
                model.UserName = userName;
                model.Email = usr.Email;
                model.CurrentEmail = usr.Email;
                //model.Password = usr.GetPassword();
                //model.PasswordConfirm = model.Password;
                model.Roles = (await userManager.GetRolesAsync(usr)).ToList();
                if (!string.IsNullOrEmpty(usr.PuckStartNodeIds))
                    model.StartNodes = usr.PuckStartNodeIds
                        .Split(',',StringSplitOptions.RemoveEmptyEntries)
                        .Select(x=>new PuckReference {Id=Guid.Parse(x) })
                        .ToList();
                model.UserVariant = usr.PuckUserVariant;
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles=PuckRoles.Users,AuthenticationSchemes =Mvc.AuthenticationScheme)]
        public async Task<JsonResult> Edit(PuckUserViewModel user,bool edit)
        {
            bool success = false;
            string message = "";
            string startPaths = "";
            string startNodeIds = "";
            var model = new PuckUserViewModel();
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("model invalid.");
                if (!edit) {
                    if (string.IsNullOrEmpty(user.Password))
                        throw new Exception("please enter a password");

                    var puser = new PuckUser
                    {
                        PuckFirstName = user.FirstName,
                        PuckSurname = user.Surname,
                        Email = user.Email,
                        UserName = user.UserName,
                        PuckUserVariant = user.UserVariant,
                        PuckStartNodeIds = user.StartNodes == null ? null : string.Join(",", user.StartNodes.Select(x => x.Id.ToString()))
                    };
                    var result = await userManager.CreateAsync(puser, user.Password);
                    if (!result.Succeeded) {
                        message = string.Join(" ", result.Errors.Select(x => x.Description));
                        throw new Exception(message);
                    }
                    if (user.Roles != null && user.Roles.Count > 0)
                    {
                        await userManager.AddToRolesAsync(puser, user.Roles.ToArray());                        
                    }
                    if (!await userManager.IsInRoleAsync(puser, PuckRoles.Puck))
                    {
                        await userManager.AddToRoleAsync(puser, PuckRoles.Puck);
                    }
                    success = true;
                }
                else
                {
                    var puser = await userManager.FindByEmailAsync(user.CurrentEmail);
                    if (puser == null)
                        throw new Exception("could not find user for edit");

                    if (!puser.Email.Equals(user.Email))
                    {
                        puser.Email = user.Email;
                    }
                    if (!puser.UserName.Equals(user.UserName))
                    {
                        puser.UserName = user.UserName;
                    }
                    user.Roles = user.Roles ?? new List<string>();
                    var roles = (await userManager.GetRolesAsync(puser)).ToList();
                    List<string> rolesToAdd = new List<string>();
                    List<string> rolesToRemove = new List<string>();
                    //get roles to remove
                    foreach (var r in roles) {
                        if (!user.Roles.Contains(r))
                            rolesToRemove.Add(r);
                    }
                    //get roles to add
                    foreach (var r in user.Roles) {
                        if (!roles.Contains(r))
                            rolesToAdd.Add(r);
                    }
                    rolesToRemove.RemoveAll(x => x.Equals(PuckRoles.Puck));
                    if (rolesToRemove.Count > 0)
                    {
                        await userManager.RemoveFromRolesAsync(puser, rolesToRemove.ToArray());
                    }
                    if (rolesToAdd.Count > 0) {
                        await userManager.AddToRolesAsync(puser, rolesToAdd);
                    }
                    
                    if (!await userManager.IsInRoleAsync(puser,PuckRoles.Puck))
                    {
                        await userManager.AddToRoleAsync(puser, PuckRoles.Puck);                        
                    }
                    
                    if (user.StartNodes == null || user.StartNodes.Count == 0)
                    {
                        puser.PuckStartNodeIds = null;
                    }
                    else
                    {
                        Guid[] startIds = user.StartNodes.Select(x => x.Id).ToArray();
                        var nodes = repo.GetPuckRevision().Where(x => startIds.Contains(x.Id) && x.Current).ToList().GroupBy(x => x.Id);
                        if (nodes.Any())
                        {
                            startPaths = string.Join(",", nodes.Select(x => x.First().Path));
                            startNodeIds = string.Join(",", nodes.Select(x => x.First().Id.ToString()));
                        }
                        puser.PuckStartNodeIds = string.Join(",", startIds.Select(x => x.ToString()));
                    }
                    if (!string.IsNullOrEmpty(user.UserVariant))
                    {
                        puser.PuckUserVariant = user.UserVariant;
                    }
                    puser.PuckFirstName = user.FirstName;
                    puser.PuckSurname = user.Surname;
                    await userManager.UpdateAsync(puser);

                    if (!string.IsNullOrEmpty(user.Password))
                    {
                        var token = await userManager.GeneratePasswordResetTokenAsync(puser);
                        var result = await userManager.ResetPasswordAsync(puser, token, user.Password);

                        if (result.Succeeded)
                            success = true;
                        else
                        {
                            success = false;
                            message = string.Join(" ", result.Errors.Select(x => x.Description));
                        }
                    }
                    else
                    {
                        success = true;
                    }
                    cache.Set<bool?>($"renewPuckClaims{user.UserName}",true);
                }

                
            }
            catch (Exception ex) {
                log.Log(ex);
                success = false;
                message = ex.Message;
            }
            return Json(new {success=success,message=message,startPaths=startPaths,startNodeIds=startNodeIds });
        }

        [Authorize(Roles =PuckRoles.Users,AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public async Task<JsonResult> Delete(string username) {
            bool success = false;
            string message = "";
            try
            {
                if (username == User.Identity.Name)
                    throw new Exception("you cannot delete your own user");
                var puser = await userManager.FindByNameAsync(username);
                if (puser == null)
                    throw new Exception("user not found");
                await userManager.DeleteAsync(puser);    
                success = true;
            }
            catch (Exception ex)
            {
                log.Log(ex);
                success = false;
                message = ex.Message;
            }
            return Json(new { success = success, message = message });
        }
    }
}
