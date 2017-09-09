﻿using Microsoft.AspNet.SignalR;
using SimBankSite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SimBankSite.SignalR_Hubs
{
    public class CommandHub : Hub
    {
        static List<ClientComm> clientComms = new List<ClientComm>();
        static ClientComm Host = null;
        string cmd = "{\"Destination\": \"8970199170310344443\",\"Command\": \"WaitSms\",\"Pars\": [\"SearchByNumber\", \"My Beeline\" ]}";

        SimContext db;

        

        public CommandHub()
        {
            db = new SimContext();
        }

        public void PrintClientsId()
        {

        }

        public void DetermineLength(string message)
        {
            string newMessage = string.Format(@"{0} has a length of: {1}", message, message.Length);
            Clients.All.ReceiveLength(newMessage);
        }

        /// <summary>
        /// Сетод принимающий список активных сим-карт
        /// </summary>
        /// <param name="json"></param>
        public void ManagerInfo(string json)
        {
            Clients.All.ComsInfoArrived(json);
            List<ActiveSim> activeComs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ActiveSim>>(json); // надеюсь, прокатит

            using (SimStorageContext db = new SimStorageContext())
            {
                foreach (var comm in activeComs)
                {
                    comm.State = SimState.Ready;
                    
                    var sim = db.AllSimCards.Find(comm.Id);
                    if (sim != null)
                    {
                        comm.UsedServicesArray = sim.UsedServices.Split(',');
                    }
                }
                db.AllSimCards.AddRange(activeComs);
                db.SaveChanges();
            }
            db.ActiveSimCards.AddRange(activeComs);
            db.SaveChanges();
        }

        /// <summary>
        /// Реакция на полученное сообщение
        /// </summary>
        /// <param name="message"></param>
        public void SmsReceived(string message)
        {
            Clients.Client(Host.ConnectionId).SmsContentReceived(message);
        }

        /// <summary>
        /// Метод выполняемый при подключении к хабу
        /// </summary>
        /// <param name="commName"></param>
        public void Connect(string commName)
        {
            var websiteKey = "8nkCH0iXXkNBgw3V";

            var id = Context.ConnectionId;

            if (commName == websiteKey)
            {
                Host = new ClientComm { ConnectionId = id, Name = commName };
            }
            else if (!clientComms.Any(x => x.ConnectionId == id))
            {
                clientComms.Add(new ClientComm { ConnectionId = id, Name = commName });
            }

            // Посылаем сообщение текущему пользователю
            Clients.Caller.onConnected(id, commName);

            // и сайту
            if (Host != null)
            {
                Clients.Client(Host.ConnectionId).newClientConnected(id, commName);
            }
        }

        /// <summary>
        /// Действие при отключении
        /// </summary>
        /// <param name="stopCalled"></param>
        /// <returns></returns>
        public override Task OnDisconnected(bool stopCalled)
        {
            var item = clientComms.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            var id = Context.ConnectionId;
            if (item != null)
            {
                clientComms.Remove(item);
                Clients.All.onUserDisconnected(id, item.Name);
            }
            // удаляем из базы все симки с этого блока
            using (SimContext db = new SimContext())
            {
                var range = db.ActiveSimCards.Where(x => x.SimBankId == id && x.State==SimState.Ready);// здесь спорный момент
                db.ActiveSimCards.RemoveRange(range);
                db.SaveChanges();
            }

            return base.OnDisconnected(stopCalled);
        }

        /// <summary>
        /// Отправить команду всем модемам
        /// </summary>
        /// <param name="cmd"></param>
        public void SendCommCommand(string cmd)
        {
            //вызываем у всех кроме отправителя метод CommandArrived
            Clients.AllExcept(Context.ConnectionId).CommandArrived(cmd);
            //вызываем у отправителя метод Confirm чтобы подтвердить что команда принята
            //Clients.Caller.Confirm();
        }

        ~CommandHub()
        {
            db.Dispose();
        }
    }
}