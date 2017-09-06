using Microsoft.AspNet.Identity.EntityFramework;
using SimBankSite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SimBankSite.Controllers
{
    public class HomeController : Controller
    {
       
        public ActionResult Index()
        {
            List<ApplicationUser> user = new List<ApplicationUser>();
            using (ServiceContext db = new ServiceContext())
            {
                ViewBag.Services = db.Services.ToList();
            }

            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                ViewBag.Users = db.Users.ToList();
            }
            
            return View();
        }

        [Authorize]
        public ActionResult About()
        {
            ViewBag.Message = "Ваша статистика";
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                ViewBag.User = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
            }

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Help()
        {
            ViewBag.Message = "Тут будет страница помощи";

            return View();
        }

        /// <summary>
        /// Добавление сервисов
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddService()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddService(Service svc)
        {
            ServiceContext db = new ServiceContext();
            db.Services.Add(svc);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}