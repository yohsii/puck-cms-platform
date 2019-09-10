using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using puck.core.Models;
using puck.core.Abstract;
using puck.core.Constants;
using Newtonsoft.Json;
using puck.core.Entities;
using puck.core.Filters;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

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
        //IAuthenticationManager authenticationManager;
        public AdminController(I_Content_Indexer i, I_Content_Searcher s, I_Log l, I_Puck_Repository r,RoleManager<PuckRole> rm,UserManager<PuckUser> um, SignInManager<PuckUser> sm/*,IAuthenticationManager authenticationManager*/) {
            this.indexer = i;
            this.searcher = s;
            this.log = l;
            this.repo = r;
            this.roleManager = rm;
            this.userManager = um;
            this.signInManager = sm;
        }

        [HttpGet]
        public ActionResult In() {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> In(puck.core.Models.LogIn user,string returnUrl) {
            var result = await this.signInManager.PasswordSignInAsync(user.Username, user.Password, user.PersistentCookie, false);
            if (result.Succeeded)
            {
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

        [Authorize(Roles =PuckRoles.Users,AuthenticationSchemes = Mvc.AuthenticationScheme)]
        public async Task<ActionResult> Index()
        {
            var model = new List<PuckUserViewModel>();
            var puckRole = await roleManager.FindByNameAsync(PuckRoles.Puck);
            var userCollection = repo.GetPuckUser().Where(x => x.Roles.Any(xx => xx.RoleId == puckRole.Id)).ToList();
            
            foreach (PuckUser pu in userCollection) {
                var puvm = new PuckUserViewModel();
                puvm.User = pu;
                puvm.Roles = (await userManager.GetRolesAsync(pu)).ToList();
                if(pu.StartNodeId!=Guid.Empty)
                    puvm.StartNode =new List<PuckPicker>{ new PuckPicker {Id=pu.StartNodeId } };
                puvm.UserVariant = pu.UserVariant;
                
                model.Add(puvm);
            }
            return View(model);
        }

        [Authorize(Roles =PuckRoles.Users, AuthenticationSchemes =Mvc.AuthenticationScheme)]
        public async Task<ActionResult> Edit(string userName=null) {
            var model = new PuckUserViewModel();
            ViewBag.Level0Type = typeof(PuckUserViewModel);
            if (!string.IsNullOrEmpty(userName)) {
                var usr = await userManager.FindByNameAsync(userName);
                model.UserName = userName;
                model.Email = usr.Email;
                model.CurrentEmail = usr.Email;
                //model.Password = usr.GetPassword();
                //model.PasswordConfirm = model.Password;
                model.Roles = (await userManager.GetRolesAsync(usr)).ToList();
                if(usr.StartNodeId!=Guid.Empty)
                    model.StartNode = new List<PuckPicker>{ new PuckPicker { Id=usr.StartNodeId} };
                model.UserVariant = usr.UserVariant;
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles=PuckRoles.Users,AuthenticationSchemes =Mvc.AuthenticationScheme)]
        public async Task<JsonResult> Edit(PuckUserViewModel user,bool edit)
        {
            bool success = false;
            string message = "";
            string startPath = "/";
            Guid startNodeId = Guid.Empty;
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
                        Email = user.Email,
                        UserName = user.UserName,
                        UserVariant = user.UserVariant,
                        StartNodeId = user.StartNode?.FirstOrDefault()?.Id ?? Guid.Empty
                    };
                    var result = await userManager.CreateAsync(puser, user.Password);
                    if (!result.Succeeded) {
                        message = string.Join(" ",result.Errors);
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

                    var roles = (await userManager.GetRolesAsync(puser)).ToList();
                    //never remove Puck role
                    if (roles != null && roles.Contains(PuckRoles.Puck))
                    {
                        roles.RemoveAll(x => x.Equals(PuckRoles.Puck));                        
                    }
                    if (roles.Count > 0)
                    {
                        await userManager.RemoveFromRolesAsync(puser, roles.ToArray());
                    }
                    if (user.Roles != null && user.Roles.Count > 0)
                    {
                        if (user.Roles.Count > 0) {
                            var rolesToAdd = user.Roles.Where(x => x != PuckRoles.Puck).ToArray();
                            await userManager.AddToRolesAsync(puser, rolesToAdd);
                        }
                    }
                    if (!await userManager.IsInRoleAsync(puser,PuckRoles.Puck))
                    {
                        await userManager.AddToRoleAsync(puser, PuckRoles.Puck);                        
                    }
                    
                    if (user.StartNode == null || user.StartNode.Count == 0)
                    {
                        puser.StartNodeId = Guid.Empty;
                    }
                    else
                    {
                        Guid picked_id = user.StartNode.First().Id;
                        var revision = repo.GetPuckRevision().Where(x => x.Id == picked_id && x.Current).FirstOrDefault();
                        if (revision != null)
                            startPath = revision.Path + "/";
                        puser.StartNodeId = picked_id;
                    }
                    if (!string.IsNullOrEmpty(user.UserVariant))
                    {
                        puser.UserVariant = user.UserVariant;
                    }
                    await userManager.UpdateAsync(puser);

                    if (!string.IsNullOrEmpty(user.Password))
                    {
                        var token = await userManager.GeneratePasswordResetTokenAsync(puser);
                        var result = await userManager.ResetPasswordAsync(puser, token, user.Password);
                    }
                    startNodeId = puser.StartNodeId;
                    success = true;
                }

                
            }
            catch (Exception ex) {
                log.Log(ex);
                success = false;
                message = ex.Message;
            }
            return Json(new {success=success,message=message,startPath=startPath,startNodeId=startNodeId });
        }

        [Authorize(Roles =PuckRoles.Users,AuthenticationSchemes =Mvc.AuthenticationScheme)]
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
