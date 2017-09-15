﻿using SimBankSite.Models;
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

        private List<OrderAndService> orderAndService = new List<OrderAndService>();

        public OrdersController()
        {
        }

        private void GetUserOrdersAndServices()
        {
            orderAndService = db.Orders.Join(db.Services, orders => orders.Service.Id, service => service.Id, (orders, service) => new OrderAndService { Order = orders, Service = service }).Where(o => o.Order.CustomerId == CurrentUser.Id).ToList();
        }

        public new void Execute(RequestContext requestContext)
        {
            string url = Request.Url.ToString();
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
                GetUserOrdersAndServices();

            }
        }
        
        [Authorize]
        public ActionResult Index()
        {
            GetCurrentUserInfo();      

            return View("Index", orderAndService);
        }

        [Authorize]
        public ActionResult OrdersPartial(string search, int searchType = 1)
        {
            GetCurrentUserInfo();
            List<OrderAndService> myOrders = new List<OrderAndService>();
            if (!string.IsNullOrEmpty(search))
            {
                switch (searchType)
                {
                    case 1:
                        myOrders = orderAndService.FindAll(order => (order.Service.Name.ToLower().Contains(search.ToLower())));
                        break;
                    case 2:
                        myOrders = orderAndService.FindAll(order => order.Order.TelNumber.Contains("985"));
                        break;
                    


                }
                


                    //o => o.Order.DateCreated.ToString().Contains(search.ToLower()) ||
                    //o.Order.Id.ToString().ToLower().Contains(search.ToLower()) || 
                    //o.Order.Message.ToLower().Contains(search.ToLower()) || 
                    //o.Service.Name.ToLower().Contains(search.ToLower()) || 
                    //o.Order.TelNumber.ToLower().Contains(search.ToLower())||
                    //o.Order.Status.ToLower().Contains(search.ToLower())
                    //).ToList();
            }
            else
            {
                myOrders = orderAndService;
            }
            return PartialView(myOrders);
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
                        await CreateOrder(CurrentUser, svc);
                    }
                   List <OrderAndService> createService = new List<OrderAndService>();
                    createService.Add(new OrderAndService { Order = db.Orders.Where(o => o.CustomerId == CurrentUser.Id).FirstOrDefault(), Service = null });
                    
                    return View("Index", createService);
                }
                else
                {
                    return Redirect("/Account/Login");
                }
            }
            else
            {
                return Redirect("/Home");
            }

        }

        /// <summary>
        /// Создает новый заказ
        /// </summary>
        /// <param name="user">Текущий пользователь</param>
        /// <param name="svc">Сервис</param>
        private async Task CreateOrder(ApplicationUser user, Service svc)
        {
            await Task.Run(async () =>
             {
                 var dt = DateTime.Now;
                 var order = new Order()
                 {
                     CustomerId = user.Id,
                     Service = svc,
                     Status = "Обработка заказа",
                     DateCreated = dt
                 };

                 user.Money -= svc.Price;
                 //await UserManager.UpdateAsync(user);
                 db.Orders.Add(order);
                 await db.SaveChangesAsync();
                 await ParseOrder(order);
             });

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
            _hub.On<string, int>("SmsContentReceived", (sms, cmdId) => UpdateOrderMessage(cmdId, sms));
        }

        private void DisconnectHub()
        {
            connection.Stop();
        }

        /// <summary>
        /// Обработка заказа
        /// </summary>
        /// <param name="order">Заказ</param>
        public async Task ParseOrder(Order order)
        {
            await Task.Run(async () =>
            {
                var serviceName = order.Service.Name;
                var sim = await GetNumberForService(serviceName);
                if (sim == null)
                {
                    order.Status = "Ошибка! Нет доступных номеров";
                    db.Entry(order).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
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
                InitializeConnection("http://151.248.112.29/");
                Subscribe();
                //вызываем метод
                _hub.Invoke("SendCommCommand", cmd).Wait();
                order.TelNumber = sim.TelNumber;
                db.SaveChanges();
            });
        }

        /// <summary>
        /// Обновляет значение сообщения у <see cref="Order"/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sms"></param>
        private void UpdateOrderMessage(int id, string sms)
        {
            // Обновляем данные заказа
            var order = db.Orders.Find(id);
            order.Status = "Ответ получен";
            order.Message = sms;

            // Обновляем данные сим-карыты
            var sim = db.AllSimCards.FirstOrDefault(s => s.TelNumber == order.TelNumber);
            sim.UsedServices += order.Service.Name + ",";
            sim.State = SimState.Ready;

            // сохраняем
            db.Entry(order).State = System.Data.Entity.EntityState.Modified;
            db.Entry(sim).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
        }

        /// <summary>
        /// Выбор номера телефона для регистрации
        /// </summary>
        /// <param name="service">Сервис</param>
        /// <returns></returns>
        public async Task<Sim> GetNumberForService(string service)
        {
            return await Task.Run(async () =>
             {
                 var list = db.AllSimCards.Where(x => x.State != SimState.Disconnected).OrderByDescending(s => System.Data.Entity.SqlServer.SqlFunctions.DataLength(s.UsedServices));

                 for (int i = 0; i < list.ToList().Count; i++)
                 {
                     var sim = list.ToList()[i];
                     if (!sim.UsedServicesArray.Contains(service) && sim.State == SimState.Ready)
                     {
                         sim.State = SimState.InUse;
                         db.Entry(sim).State = System.Data.Entity.EntityState.Modified;
                         await db.SaveChangesAsync();
                         return sim;
                     }
                 }

                 ModelState.AddModelError("", "Не найдены активные симкарты");
                 return null;
             });
        }

        ~OrdersController()
        {
            db.Dispose();
        }
    }

    /// <summary>
    /// Класс, представляющий собой команду клиенту
    /// </summary>
    public class CommandClass
    {
        public int Id { get; set; }
        public string Destination { get; set; }
        public string Command { get; set; }
        public string[] Pars { get; set; }
    }
}
