using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SimBankSite.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;

namespace SimBankSite.Controllers.Admin
{   
    [Authorize(Roles ="Admin")]
    public class ApplicationUsersController : Controller
    {
        //private ApplicationDbContext db = new ApplicationDbContext();

        private ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        // GET: ApplicationUsers
        public async Task<ActionResult> Index()
        {
            List<ApplicationUser> model = new List<ApplicationUser>();
            model.AddRange(UserManager.Users.ToList());
            return View(model);
        }

        // GET: ApplicationUsers/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser applicationUser = await UserManager.FindByIdAsync(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }
            return View(applicationUser);
        }

        // GET: ApplicationUsers/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ApplicationUsers/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Money,Email,EmailConfirmed,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName")] ApplicationUser applicationUser, string pass)
        {
            if (ModelState.IsValid)
            {
                var res = await UserManager.CreateAsync(new ApplicationUser(), pass);

                if (res.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrors(res);
                }
            }

            return View(applicationUser);
        }

        // GET: ApplicationUsers/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser applicationUser = await UserManager.FindByIdAsync(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }
            return View(applicationUser);
        }

        // POST: ApplicationUsers/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Money,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName")] ApplicationUser applicationUser)
        {
            if (ModelState.IsValid)
            {
                var savedUser = await UserManager.FindByIdAsync(applicationUser.Id);

                savedUser.AccessFailedCount = applicationUser.AccessFailedCount;
                savedUser.Email = applicationUser.Email;
                savedUser.EmailConfirmed = applicationUser.EmailConfirmed;
                savedUser.LockoutEnabled = applicationUser.LockoutEnabled;
                savedUser.LockoutEndDateUtc = applicationUser.LockoutEndDateUtc;
                savedUser.Money = applicationUser.Money;
                //savedUser.PasswordHash = applicationUser.PasswordHash;
                savedUser.PhoneNumber = applicationUser.PhoneNumber;
                savedUser.PhoneNumberConfirmed = applicationUser.PhoneNumberConfirmed;
                savedUser.Transactions = applicationUser.Transactions;
                savedUser.TwoFactorEnabled = applicationUser.TwoFactorEnabled;
                savedUser.UserName = applicationUser.UserName;


                var res = await UserManager.UpdateAsync(applicationUser);
                if (res.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrors(res);
                }                
            }
            return View(applicationUser);
        }

        // GET: ApplicationUsers/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser applicationUser = await UserManager.FindByIdAsync(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }
            var res = await UserManager.DeleteAsync(applicationUser);
            if (res.Succeeded)
            {
                return RedirectToAction("Index");
            }
            else
            {
                AddErrors(res);
            }
            return View();
        }

        //// POST: ApplicationUsers/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> DeleteConfirmed(string id)
        //{
        //    ApplicationUser applicationUser = await db.ApplicationUsers.FindAsync(id);
        //    db.ApplicationUsers.Remove(applicationUser);
        //    await db.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}

        private void AddErrors(IdentityResult res)
        {
            foreach (string error in res.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
    }
}
