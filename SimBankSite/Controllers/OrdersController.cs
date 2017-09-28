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
using System.Diagnostics;
using PagedList.Mvc;
using PagedList;

namespace SimBankSite.Controllers
{
    public class OrdersController : Controller
    {
        private IHubProxy _hub;
        private HubConnection connection;

        private ApplicationDbContext db = new ApplicationDbContext(); // ACHTUNG!!!

        private ApplicationUserManager UserManager { get; set; }
        private ApplicationUser CurrentUser { get; set; }

        private IEnumerable<OrderAndService> orderAndService;

        /// <summary>
        /// Проверка объектов на устаревание, в случае если ответа нет 5 минут , освобождает номер
        /// </summary>
        private void CheckOrdersState()
        {
            List<OrderAndService> forCheckState = orderAndService.Where(o => o.Order.Status != "Ответ получен" && o.Order.Status != "Ошибка получения данных")
                                                                 .Where(o => o.Order.DateCreated + TimeSpan.FromMinutes(5) < DateTime.Now).ToList();

            for (int i = 0; i < forCheckState.Count; i++)
            {
                forCheckState[i].Order.Status = "Ошибка получения данных";

                db.AllSimCards.FirstOrDefault(s => s.TelNumber == forCheckState[i].Order.TelNumber).State = SimState.Ready;

                var user = UserManager.FindById(forCheckState[i].Order.CustomerId);
                user.Money += forCheckState[i].Service.Price; //Возвращаем деньги

                db.Entry(forCheckState[i].Order).State = System.Data.Entity.EntityState.Modified;
                //db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                UserManager.Update(user);
                db.SaveChanges();
;
            }
        }

        private void GetUserOrdersAndServices()
        {
            orderAndService = db.Orders.Join(
                db.Services, orders => orders.Service.Id,
                service => service.Id,
                (orders, service) => new OrderAndService
                {
                    Order = orders,
                    Service = service
                }).Where(o => o.Order.CustomerId == CurrentUser.Id);
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
        public ActionResult Index(int? page = 1)
        {
           
            int pageSize = 40;
            int pageNumber = page ?? 1;


            GetCurrentUserInfo();

            CheckOrdersState();

            var pagedList = orderAndService.OrderByDescending(o => o.Order.DateCreated).ToList().ToPagedList(pageNumber, pageSize);

            return View("Index", pagedList);
        }



        [Authorize]
        public ActionResult OrdersPartial(int? page)
        {

            int pageSize = 40;
            int pageNumber = page ?? 1;

            GetCurrentUserInfo();
            
            CheckOrdersState();

            var pagedList = orderAndService.OrderByDescending(o => o.Order.DateCreated).ToList().ToPagedList(pageNumber, pageSize);
            return PartialView("OrdersPartial", pagedList);
        }

        [HttpPost]
        [Authorize]
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
                   List<OrderAndService> createService = new List<OrderAndService>();
                    createService.Add(new OrderAndService { Order = db.Orders.Where(o => o.CustomerId == CurrentUser.Id).FirstOrDefault(), Service = null });
                    
                    return View("Index", createService.OrderByDescending(o => o.Order.DateCreated).AsEnumerable().ToPagedList(1,1));
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
        /// Пожаловаться на плохую симку
        /// </summary>
        /// <param name="id"> номер заказа</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public ActionResult Report(int? id)
        {
            if (id==null)
            {
                return HttpNotFound();
            }

            //Обработка

            return View("Index");
        }

        #region Работа с коробкой
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
                 await UserManager.UpdateAsync(user);
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
                CommandClass command;
                if (!string.IsNullOrEmpty(order.Service.SenderNumber))
                {
                    //создаем команду ТИП ОПРЕДЕЛЯЕМ ТУТ
                    command = new CommandClass
                    {
                        Id = order.Id,
                        Destination = sim.Id,
                        Command = "WaitSms",
                        Pars = new string[] { "SearchByNumber", order.Service.SenderNumber }
                    };
                }
                else
                {
                    //создаем команду ТИП ОПРЕДЕЛЯЕМ ТУТ
                    command = new CommandClass
                    {
                        Id = order.Id,
                        Destination = sim.Id,
                        Command = "WaitSms",
                        Pars = new string[] { "ReceiveLast" }
                    };
                }
                //превращаем ее в JSON
                string cmd = JsonConvert.SerializeObject(command);
                //подключаемся
                InitializeConnection("http://simsimsms.ru/");
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

                 ModelState.AddModelError("", "Не найдено активных симкарт");
                 return null;
             });
        }

        ~OrdersController()
        {
            db.Dispose();
        }
    }
    #endregion

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
