using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.ComponentModel.DataAnnotations;
using SimBankSite.Models;
using System.Linq;
using System.Collections.Generic;

namespace SimBankSite.Controllers.Admin
{
    [Authorize]//(Roles = "Admin")]
    public class RoleAdminController : Controller
    {
        private ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        private ApplicationRoleManager RoleManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationRoleManager>();
            }
        }

        // GET: RoleAdmin
        public ActionResult Index()
        {
            return View(RoleManager.Roles);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create([Required]string name)
        {
            if(ModelState.IsValid)
            {
                var res = await RoleManager.CreateAsync(new ApplicationRole(name));

                if(res.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrors(res);
                }
            }

            return View(name);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(string id)
        {
            ApplicationRole role = await RoleManager.FindByIdAsync(id);
            if (role != null)
            {
                var res = await RoleManager.DeleteAsync(role);

                if (res.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return View("Error", res.Errors);
                }
            }
            else
            {
                return View("Error", new string[] { "Роль не найдена " });
            }
        }

        public async Task<ActionResult> Edit(string id)
        {
            ApplicationRole role = await RoleManager.FindByIdAsync(id);

            string[] memberIDs = role.Users.Select(x => x.UserId).ToArray();

            IEnumerable<ApplicationUser> members = UserManager.Users.Where(x => memberIDs.Any(y =>y == x.Id));

            IEnumerable<ApplicationUser> notMembers = UserManager.Users.Except(members);

            return View(new RoleEditModel
            {
                Role = role,
                Members = members,
                NotMembers = notMembers
            });
        }

        [HttpPost]
        public async Task<ActionResult> Edit(RoleModificationModel model)
        {
            IdentityResult result;
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                if (ModelState.IsValid)
                {
                    foreach (string userId in model.IdsToAdd ?? new string[] { })
                    {
                        result = await UserManager.AddToRoleAsync(userId, model.RoleName);
                        //var user = await UserManager.FindByIdAsync(userId);
                        //await UserManager.UpdateAsync(user);

                        if (!result.Succeeded)
                        {
                            return View("Error", result.Errors);
                        }
                    }

                    foreach (string userId in model.IdsToDelete ?? new string[] { })
                    {
                        result = await UserManager.RemoveFromRolesAsync(userId, model.RoleName);

                        if (!result.Succeeded)
                        {
                            return View("Error", result.Errors);
                        }
                    }

                    return RedirectToAction("Index");
                }

                return View("Error", new string[] { "Роль не найдена" });
            }
        }

        private void AddErrors(IdentityResult res)
        {
            foreach(string error in res.Errors)
            {
                ModelState.AddModelError("",error);
            }
        }
    }
}