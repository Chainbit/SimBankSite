using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.ComponentModel.DataAnnotations;
using SimBankSite.Models;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;

namespace SimBankSite.Controllers.Admin
{
    
    public class RoleAdminController : Controller
    {
        ApplicationDbContext context = new ApplicationDbContext();

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

        private RoleManager<IdentityRole> IdentityRoleManager
        {
            get
            {
                return new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            }
        }

        // GET: RoleAdmin
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            List<IdentityRole> model = new List<IdentityRole>();
            model.AddRange(IdentityRoleManager.Roles.ToList());
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]        
        public async Task<ActionResult> Create([Required]string name)
        {
            if(ModelState.IsValid)
            {
                var res = await IdentityRoleManager.CreateAsync(new IdentityRole(name));

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
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(string id)
        {
            IdentityRole role = await IdentityRoleManager.FindByIdAsync(id);
            if (role != null)
            {
                var res = await IdentityRoleManager.DeleteAsync(role);

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

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(string id)
        {
            IdentityRole role = await IdentityRoleManager.FindByIdAsync(id);

            List<string> memberIDs = role.Users.Select(x => x.UserId).ToList();

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
        [Authorize(Roles = "Admin")]
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