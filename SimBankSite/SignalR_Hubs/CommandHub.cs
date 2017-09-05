using Microsoft.AspNet.SignalR;
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

        public void DetermineLength(string message)
        {
            string newMessage = string.Format(@"{0} has a length of: {1}", message, message.Length);
            Clients.All.ReceiveLength(newMessage);
        }

        public void GetClients()
        {
            foreach (var item in Clients.All)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine("done");
        }

        public void ManagerInfo(string json)
        {
            Clients.Client(Host.ConnectionId).ComsInfoArrived(json);
        }

        /// <summary>
        /// Реакция на полученное сообщение
        /// </summary>
        /// <param name="message"></param>
        public void SmsReceived(string message)
        {
            Console.WriteLine(message);
        }

        public void PrintClientsId()
        {
            foreach (var client in clientComms)
            {
                Console.WriteLine(client.ConnectionId);
            }
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
            if (item != null)
            {
                clientComms.Remove(item);
                var id = Context.ConnectionId;
                Clients.All.onUserDisconnected(id, item.Name);
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
    }
}