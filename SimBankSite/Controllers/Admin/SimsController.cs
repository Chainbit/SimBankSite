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

namespace SimBankSite.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class SimsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Sims
        public async Task<ActionResult> Index()
        {
            return View(await db.AllSimCards.ToListAsync());
        }

        // GET: Sims/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sim sim = await db.AllSimCards.FindAsync(id);
            if (sim == null)
            {
                return HttpNotFound();
            }
            return View(sim);
        }

        //// GET: Sims/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: Sims/Create
        //// Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        //// сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create([Bind(Include = "Id,TelNumber,UsedServices,SimBankId,State")] Sim sim)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.AllSimCards.Add(sim);
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    return View(sim);
        //}

        // GET: Sims/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sim sim = await db.AllSimCards.FindAsync(id);
            if (sim == null)
            {
                return HttpNotFound();
            }
            return View(sim);
        }

        // POST: Sims/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,TelNumber,UsedServices,SimBankId,State")] Sim sim)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sim).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(sim);
        }

        // GET: Sims/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sim sim = await db.AllSimCards.FindAsync(id);
            if (sim == null)
            {
                return HttpNotFound();
            }
            return View(sim);
        }

        // POST: Sims/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            Sim sim = await db.AllSimCards.FindAsync(id);
            db.AllSimCards.Remove(sim);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
