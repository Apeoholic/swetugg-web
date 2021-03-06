﻿using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;

namespace Swetugg.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Administrator")]
    [RouteArea("Admin", AreaPrefix = "admin")]
    [RoutePrefix("users")]
    public class UsersController : Controller
    {
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public UsersController()
        {
        }

        /*public UsersController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }*/

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }


        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        [Route("")]
        public async Task<ActionResult> Index()
        {
            var users = await UserManager.Users.ToListAsync();
            var roles = await RoleManager.Roles.ToListAsync();
            
            ViewBag.Roles = roles;
            
            return View(users);
        }

        [HttpPost]
        [Route("set-roles")]
        public async Task<ActionResult> SetRoles(string userId, List<string> roles)
        {
            var currentRoles = await UserManager.GetRolesAsync(userId);
            roles = roles ?? new List<string>();
            if (currentRoles != null)
            {
                foreach (var currentRole in currentRoles)
                {
                    if (roles.Contains(currentRole))
                    {
                        // User is already in role, no need to add again
                        roles.Remove(currentRole);
                    }
                    else
                    {
                        // User should no longer be in this role
                        await UserManager.RemoveFromRoleAsync(userId, currentRole);

                    }
                }
            }
            var result = await UserManager.AddToRolesAsync(userId, roles.ToArray());

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("delete")]
        public async Task<ActionResult> Delete(string userId)
        {
            var user = await UserManager.Users.FirstAsync(u => u.Id == userId);
            var result = await UserManager.DeleteAsync(user);

            return RedirectToAction("Index");
        }
    }
}