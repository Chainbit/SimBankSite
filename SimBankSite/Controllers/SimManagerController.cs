using SimBankSite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.SignalR;
using SimBankSite.SignalR_Hubs;
using System.Web.Mvc;
using System.Web.Routing;
using System.Security.Principal;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;

namespace SimBankSite.Controllers
{
    public class SimManagerController : IController
    {
        private IHubProxy _hub;
        private HubConnection connection;
        private SimContext db = new SimContext();

        public void Execute(RequestContext requestContext)
        {
            InitializeConnection("/");
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

        private void DisconnectHub()
        {
            connection.Stop();
        }

        /// <summary>
        /// Обработка заказа
        /// </summary>
        /// <param name="order">Заказ</param>
        public void ParseOrder(Order order)
        {
            var serviceName = order.Service.Name;
            var sim = GetNumberForService(serviceName);
            if (sim==null)
            {
                order.Status = "Ошибка! Нет доступных номеров";
                return;
            }
            //создаем команду
            CommandClass command = new CommandClass {
                Destination = sim.Id,
                Command = "WaitSms",
                Pars = new string[]{ "ReceiveLast" }
            };
            //превращаем ее в JSON
            string cmd = JsonConvert.SerializeObject(command);
            //подключаемся
            InitializeConnection("/");
            //подписываемся на события и вызываем методы
            _hub.On("SmsContentReceived", (x => order.Message = x));
            _hub.Invoke("SendCommCommand", cmd).Wait();
        }

        /// <summary>
        /// Выбор номера телефона для регистрации
        /// </summary>
        /// <param name="service">Сервис</param>
        /// <returns></returns>
        public Sim GetNumberForService(string service)
        {
            var list = db.ActiveSimCards.OrderByDescending(s => s.UsedServicesArray.Length);
            foreach (var sim in list)
            {
                if (!sim.UsedServicesArray.Contains(service))
                {
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
        public string Destination { get; set; }
        public string Command { get; set; }
        public string[] Pars { get; set; }
    }
}
