using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;

namespace SimBankSite.Helpers
{
    public static class IdentityHelpers
    {
        public static MvcHtmlString GetUserName(this HtmlHelper html, string id)
        {
            ApplicationUserManager manager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();

            return new MvcHtmlString(manager.FindByIdAsync(id).Result.UserName);
        }
    }
}