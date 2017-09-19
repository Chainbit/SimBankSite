using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using SimBankSite.Models;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using System.Reflection;

namespace SimBankSite.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private RoleManager<IdentityRole> _roleManager;

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, RoleManager<IdentityRole> roleManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

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

        public RoleManager<IdentityRole> RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().GetUserManager<RoleManager<IdentityRole>>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Ваш пароль изменен."
                : message == ManageMessageId.SetPasswordSuccess ? "Пароль задан."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Настроен поставщик двухфакторной проверки подлинности."
                : message == ManageMessageId.Error ? "Произошла ошибка."
                : message == ManageMessageId.AddPhoneSuccess ? "Ваш номер телефона добавлен."
                : message == ManageMessageId.RemovePhoneSuccess ? "Ваш номер телефона удален."
                : "";

            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);
            var roleName = GetRoleName(userId);
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId),
                User = user,
                RoleName = roleName
            };
            return View(model);
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Создание и отправка маркера
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Ваш код безопасности: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Отправка SMS через поставщик SMS для проверки номера телефона
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // Это сообщение означает наличие ошибки; повторное отображение формы
            ModelState.AddModelError("", "Не удалось проверить телефон");
            return View(model);
        }

        //
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // Это сообщение означает наличие ошибки; повторное отображение формы
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "Внешнее имя входа удалено."
                : message == ManageMessageId.Error ? "Произошла ошибка."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Запрос перенаправления к внешнему поставщику входа для связывания имени входа текущего пользователя
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        // Post: /Manage/BalanceAdding
        public ActionResult BalanceAdding()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BalanceAdding(MoneyAddOnBalance a)
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            string ptrn = "[0-9]{1,10}(\\,[0-9]{0,2})?";

            string moneyToAdd = a.Money;
            decimal money = 0;

            if (moneyToAdd.Contains("."))
            {
                moneyToAdd = moneyToAdd.Replace(".", ",");
            }

            Regex check = new Regex(ptrn);
            Match match = check.Match(moneyToAdd);
            if (match.Success)
            {
                money = decimal.Parse(match.Value);
            }

            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                if (money <= 0)
                {
                    ModelState.AddModelError("", "Сумма начисления должна быть больше нуля");
                }
                else
                {
                    user.Money += money;

                    await UserManager.UpdateAsync(user);
                    ViewBag.Balance = moneyToAdd.ToString();
                }
            }
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> YandexPartial(Transaction payment)
        {
            if (payment==null)
            {
                return HttpNotFound();
            }

            if(IsValidSum(payment.Sum))
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    payment.Date = DateTime.Now;
                    db.Transactions.Add(payment);
                    db.SaveChanges();
                    payment.AppUser = UserManager.FindById(payment.AppUser_Id);
                }
                return PartialView("YandexPartial", payment);
            }
            else
            {
                ModelState.AddModelError("", "Недопустимое значение для пополнения счета. Пожалуйста попробуйте еще раз.");
                return PartialView("BalanceAddingPart",payment);
            }
        }

        private bool IsValidSum(decimal sum)
        {
            if(sum > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [HttpGet]
        [Authorize]
        //[ValidateAntiForgeryToken]
        public ActionResult CancelPayment(int? transactionID, string userId)
        {
            if (transactionID==null)
            {
                return HttpNotFound();
            }

            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                var payment = db.Transactions.Find(transactionID);
                if (payment.AppUser_Id!=userId)
                {
                    return HttpNotFound();
                }
                db.Transactions.Remove(payment);
                db.Entry(payment).State = System.Data.Entity.EntityState.Deleted;
                db.SaveChanges();
            }

            return Redirect("/Home/Index");
        }

        [HttpPost]
        [AllowAnonymous]
        public void Paid(string notification_type, string operation_id, string label, string datetime,
        string amount, string withdraw_amount, string sender, string sha1_hash, string currency, bool codepro)
        {
            string key = "+qCr84wBkYG/Qf3td9ZVOYZt"; // секретный код
                                                     // проверяем хэш
            string paramString = string.Format("{0}&{1}&{2}&{3}&{4}&{5}&{6}&{7}&{8}",
                notification_type, operation_id, amount, currency, datetime, sender,
                codepro.ToString().ToLower(), key, label);

            var context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<SignalR_Hubs.CommandHub>();
            context.Clients.All.broadcast(string.Format("Пришел платеж: \r\n notification_type:{0}\r\n operation_id:{1}\r\n amount:{2}\r\n currency:{3}\r\n datetime:{4}\r\n sender:{5}\r\n codepro:{6}\r\n key:{7}\r\n label:{8}", 
                notification_type, operation_id, amount, currency, datetime, sender,
                codepro.ToString().ToLower(), key, label));

            string paramStringHash1 = GetHash(paramString);
            // создаем класс для сравнения строк
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            // если хэши идентичны, добавляем данные о заказе в бд
            if (0 == comparer.Compare(paramStringHash1, sha1_hash))
            {
                ApplicationUser user;

                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    Transaction payment = db.Transactions.FirstOrDefault(o => o.Id == Convert.ToInt32(label));
                    if (payment!=null)
                    {
                        
                        //payment.Operation_Id = operation_id;
                        payment.Date = DateTime.Now;
                        //payment.Sum = decimal.Parse(amount); //Зависит от того кто платит комиссию
                        payment.Sum = decimal.Parse(withdraw_amount);
                        //order.Sender = sender;
                        payment.State = PaymentStatus.Confirmed;
                        user = payment.AppUser;
                        db.Entry(payment).State = System.Data.Entity.EntityState.Modified;
                        context.Clients.All.broadcast(string.Format("Id: {0}\r\n Sum:{1}\r\n Date:{3}\r\n State:{4}", payment.Id, payment.Sum, payment.Date, payment.State));
                        db.SaveChanges();
                    }
                    else
                    {
                        context.Clients.All.broadcast("Транзакция не найдена!");
                        return;
                    }
                }
                user.Money += decimal.Parse(withdraw_amount);

                UserManager.Update(user);
            }
            else
            {
                context.Clients.All.broadcast("Хэш не сошелся!");
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public void Test(object value)
        {
            var a = Convert.ToByte(value);
            //var propInfo = typeof(Transaction).GetProperties(BindingFlags.GetField);

            
                //bool readable = propInfo[0].CanRead;
                //bool writable = propInfo[0].CanWrite;

                //Debug.WriteLine("   Property name: {0}", propInfo[0].Name);
                //Debug.WriteLine("   Property type: {0}", propInfo[0].PropertyType);
                //Debug.WriteLine("   Read-Write:    {0}", readable & writable);
                //if (readable)
                //{
                //    MethodInfo getAccessor = propInfo[0].GetMethod;
                //    Debug.WriteLine("   Visibility:    {0}",
                //                      GetVisibility(getAccessor));
                //}
                //if (writable)
                //{
                //    MethodInfo setAccessor = propInfo[0].SetMethod;
                //    Debug.WriteLine("   Visibility:    {0}",
                //                      GetVisibility(setAccessor));
                //}
               
                
            
            var context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<SignalR_Hubs.CommandHub>();
            context.Clients.All.broadcast(a);
        }
        public static String GetVisibility(MethodInfo accessor)
        {
            if (accessor.IsPublic)
                return "Public";
            else if (accessor.IsPrivate)
                return "Private";
            else if (accessor.IsFamily)
                return "Protected";
            else if (accessor.IsAssembly)
                return "Internal/Friend";
            else
                return "Protected Internal/Friend";
        }

        public string GetHash(string val)
        {
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] data = sha.ComputeHash(Encoding.Default.GetBytes(val));

            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Вспомогательные приложения
        // Используется для защиты от XSRF-атак при добавлении внешних имен входа
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private string GetRoleName(string userId)
        {
            string roleName = "Роль не найдена";
            var res = UserManager.GetRolesAsync(userId);
            if(res.Result.Count > 0)
            {
                roleName = res.Result[0];
            }

            return roleName;
        }


        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

        #endregion
    }
}