using SimBankSite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNet.SignalR;
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
        private SimContext SimDb = new SimContext();
        private ApplicationUserManager UserManager { get; set; }

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

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(int? id)
        {
            id = id ?? -1;
            Service svc;
            using (ServiceContext db = new ServiceContext())
            {
                svc = await db.Services.FindAsync(id);
            }
            if (User.Identity.IsAuthenticated)
            {
                UserManager= HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user.Money>=svc.Price)
                {
                    CreateOrder(user, svc);
                }
                return View("Index");
            }
            else
            {
                return Redirect("/Account/Login");
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
            using (ServiceContext OrdersDB = new ServiceContext())
            {
                user.Money -= svc.Price;
                await UserManager.UpdateAsync(user);
                OrdersDB.Orders.Add(order);
                OrdersDB.SaveChanges();
            }
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
            using (ServiceContext db = new ServiceContext())
            {
                db.Orders.Find(id).Message = sms;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Выбор номера телефона для регистрации
        /// </summary>
        /// <param name="service">Сервис</param>
        /// <returns></returns>
        public Sim GetNumberForService(string service)
        {
            var list = SimDb.ActiveSimCards.OrderByDescending(s => s.UsedServicesArray.Length);
            foreach (var sim in list)
            {
                if (!sim.UsedServicesArray.Contains(service) && sim.State != SimState.InUse)
                {
                    sim.State = SimState.InUse;
                    SimDb.SaveChanges();
                    return sim;
                }
            }
            return null;
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
