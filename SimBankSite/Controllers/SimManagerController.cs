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
    public class SimManagerController : ApiController, IController
    {
        private IPrincipal CurrentUser;
        private IHubProxy _hub;
        private HubConnection connection;

        public void Execute(RequestContext requestContext)
        {
            CurrentUser = requestContext.HttpContext.User;
            InitializeConnection("/");
        }

        private void InitializeConnection(string ServerAddress)
        {
            connection = new HubConnection(ServerAddress);
            //ВОТ ЗДЕСЬ БЛЯТЬ ОЧЕНЬ ВНИМАТЕЛЬНО НУЖНО НАЗВАНИЕ ХАБА НАПИСАТЬ КАК НАДО
            _hub = connection.CreateHubProxy("CommandHub");

            connection.Start().Wait();
        }
    }
}
