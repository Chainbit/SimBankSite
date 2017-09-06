using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SimBankSite.Models;

namespace SimBankSite.Controllers
{
    public class ServicesController : ApiController
    {
        private ServiceContext db = new ServiceContext();

        // Project products to product DTOs.
        private IQueryable<Service> MapServices()
        {
            return from p in db.Services
                   select p;
        }

        public IEnumerable<Service> GetServices()
        {
            return MapServices().AsEnumerable();
        }

        public Service GetService(int id)
        {
            var product = (from p in MapServices()
                           where p.Id == 1
                           select p).FirstOrDefault();
            if (product == null)
            {
                throw new HttpResponseException(
                    Request.CreateResponse(HttpStatusCode.NotFound));
            }
            return product;
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
