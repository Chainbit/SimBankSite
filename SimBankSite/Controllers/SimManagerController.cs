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

        /// <summary>
        /// Получить номер для использования в сервисе
        /// </summary>
        /// <param name="service">Сервис</param>
        public void GetFreeNumber(Service service)
        {
            //выбираем номер из Бд
        }
    }
}
