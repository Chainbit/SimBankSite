using SimBankSite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNet.Identity;
using SimBankSite.SignalR_Hubs;
using System.Web.Mvc;
using System.Web.Routing;
using System.Security.Principal;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;

namespace SimBankSite.Controllers
{
    public class OrdersController : Controller
    {
        private IHubProxy _hub;
        private HubConnection connection;

        private ApplicationDbContext db = new ApplicationDbContext(); // ACHTUNG!!!

        private ApplicationUserManager UserManager { get; set; }
        private ApplicationUser CurrentUser { get; set; }

        public OrdersController()
        {
        }

        public new void Execute(RequestContext requestContext)
        {
            string url =Request.Url.ToString();
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority +
            Request.ApplicationPath.TrimEnd('/') + "/";
            InitializeConnection(url);
            Subscribe();
        }

        private void GetCurrentUserInfo()
        {
            if (User.Identity.IsAuthenticated)
            {
                UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                CurrentUser = UserManager.FindById(User.Identity.GetUserId());
            }
        }

        [Authorize]
        public ActionResult Index()
        {
            GetCurrentUserInfo();
            return View("Index", db.Orders.Where(o => o.CustomerId == CurrentUser.Id));
        }

        [HttpPost]
        public async Task<ActionResult> Create(int? value)
        {
            if (value != null)
            {                

                Service svc = db.Services.Find(value);

                if (User.Identity.IsAuthenticated)
                {
                    GetCurrentUserInfo();
                    if (CurrentUser.Money >= svc.Price)
                    {
                        CreateOrder(CurrentUser, svc);
                    }
                    return View("Index",  db.Orders.Where(o=>o.CustomerId== CurrentUser.Id));
                }
                else
                {
                    return Redirect("/Account/Login");
                }
            }
            else
            {
                return View("Error!");
            }
            
        }

        /// <summary>
        /// Создает новый заказ
        /// </summary>
        /// <param name="user">Текущий пользователь</param>
        /// <param name="svc">Сервис</param>
        private async void CreateOrder(ApplicationUser user, Service svc)
        {
            var dt = DateTime.Now;
            var order = new Order()
            {
                CustomerId = user.Id,
                Service = svc,
                Status = "Обработка заказа",
                DateCreated = dt
            };

            //using(ApplicationDbContext UsersDB = new ApplicationDbContext())
            user.Money -= svc.Price;
            await UserManager.UpdateAsync(user);
            db.Orders.Add(order);
            db.SaveChanges();

            await ParseOrder(order);
        }

        /// <summary>
        /// Создает соединение с хабом по указанному адресу (вообще говоря это просто адрес сайта но пока так)
        /// </summary>
        /// <param name="ServerAddress">Адрес хаба</param>
        private void InitializeConnection(string ServerAddress)
        {
            connection = new HubConnection(ServerAddress);
            //ВОТ ЗДЕСЬ БЛЯТЬ ОЧЕНЬ ВНИМАТЕЛЬНО НУЖНО НАЗВАНИЕ ХАБА НАПИСАТЬ КАК НАДО
            _hub = connection.CreateHubProxy("CommandHub");
            connection.Start().Wait();

            _hub.Invoke("Connect", "8nkCH0iXXkNBgw3V").Wait();
        }

        private void Subscribe()
        {
            _hub.On<string, int>("SmsContentReceived", (sms, cmdId) => UpdateOrderMessage(cmdId,sms));
        }

        private void DisconnectHub()
        {
            connection.Stop();
        }

        /// <summary>
        /// Обработка заказа
        /// </summary>
        /// <param name="order">Заказ</param>
        public Task ParseOrder(Order order)
        {
            return Task.Factory.StartNew(() =>
            {
                var serviceName = order.Service.Name;
                var sim = GetNumberForService(serviceName);
                if (sim == null)
                {
                    order.Status = "Ошибка! Нет доступных номеров";
                    return;
                }
                //создаем команду
                CommandClass command = new CommandClass
                {
                    Id = order.Id,
                    Destination = sim.Id,
                    Command = "WaitSms",
                    Pars = new string[] { "ReceiveLast" }
                };
                //превращаем ее в JSON
                string cmd = JsonConvert.SerializeObject(command);
                //подключаемся
                InitializeConnection("/");
                Subscribe();
                //вызываем метод
                _hub.Invoke("SendCommCommand", cmd).Wait();
            });
        }

        /// <summary>
        /// Обновляет значение сообщения у <see cref="Order"/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sms"></param>
        private void UpdateOrderMessage(int id, string sms)
        {
            db.Orders.Find(id).Message = sms;
            db.SaveChanges();
        }

        /// <summary>
        /// Выбор номера телефона для регистрации
        /// </summary>
        /// <param name="service">Сервис</param>
        /// <returns></returns>
        public Sim GetNumberForService(string service)
        {
            var list = db.ActiveSimCards.OrderByDescending(s => System.Data.Entity.SqlServer.SqlFunctions.DataLength(s.UsedServices));
            foreach (var sim in list)
            {
                if (!sim.UsedServicesArray.Contains(service) && sim.State != SimState.InUse)
                {
                    sim.State = SimState.InUse;
                    db.SaveChanges();
                    return sim;
                }
            }
            return null;
        }

        ~OrdersController()
        {
            db.Dispose();
        }
    }

    /// <summary>
    /// Класс, представляющий собо команду клиенту
    /// </summary>
    public class CommandClass
    {
        public int Id { get; set; }
        public string Destination { get; set; }
        public string Command { get; set; }
        public string[] Pars { get; set; }
    }
}
