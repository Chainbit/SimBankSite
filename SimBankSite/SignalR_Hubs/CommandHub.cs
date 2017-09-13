using Microsoft.AspNet.SignalR;
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

        public CommandHub()
        {
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
            List<Sim> activeComs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Sim>>(json); // надеюсь, прокатит
            List<Sim> comsToAdd = new List<Sim>();
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                foreach (var comm in activeComs)
                {
                    var existing = db.AllSimCards.Find(comm.Id);
                    if (existing != null)
                    {
                        existing.State = SimState.Ready;
                        continue;
                    }

                    //comm.SimBankId = Context.ConnectionId;
                    comm.State = SimState.Ready;
                    comm.UsedServices = "";

                    var sim = db.AllSimCards.Find(comm.Id);
                    if (sim != null)
                    {
                        comm.UsedServicesArray = sim.UsedServices.Split(',');
                    }
                    comsToAdd.Add(comm);
                }
                db.AllSimCards.AddRange(comsToAdd);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Реакция на полученное сообщение
        /// </summary>
        /// <param name="message"></param>
        public void SmsReceived(string message,int commandID)
        {
            Clients.Client(Host.ConnectionId).SmsContentReceived(message, commandID);
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

        public override Task OnReconnected()
        {
            var id = Context.ConnectionId;
            return base.OnReconnected();
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
            if (stopCalled)
            {
                // удаляем из базы все симки с этого блока
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    var range = db.AllSimCards.Where(x => x.SimBankId == item.Name && x.State==SimState.Ready);// здесь спорный момент
                    foreach (Sim comm in range)
                    {
                        comm.State = SimState.Disconnected;
                        db.Entry(comm).State = System.Data.Entity.EntityState.Modified;
                    }
                    db.SaveChanges();
                }
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
            Clients.All.CommandArrived(cmd);
            //вызываем у отправителя метод Confirm чтобы подтвердить что команда принята
            //Clients.Caller.Confirm();
        }

        ~CommandHub()
        {
            Clients.All.Disconnect();
            //db.Dispose();
        }
    }
}