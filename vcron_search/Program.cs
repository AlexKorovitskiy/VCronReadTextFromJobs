using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualCron;
using VisualCronAPI;

namespace vcron_search
{
    class Program
    {
        static void Main(string[] args)
        {
            DumpAllSql(true);
            Console.ReadLine();
        }

        private static void DumpAllSql(bool isProd = true)
        {
            var server = ConnectToServer(GetClient(), isProd);
            var connections = server.Connections.GetAll().ToDictionary(x => x.Id);
            var jobs = server.Jobs.GetAll();
            foreach (var job in jobs)
            {

                foreach (var task in job.Tasks.Where(t => t.TaskType == TaskClass.TaskT.SQL))
                {
                    var connection = connections.ContainsKey(task.SQL.ConnectionId) ? connections[task.SQL.ConnectionId] : null;
                    Console.WriteLine($"Job: {job.Name}({job.Stats.Active}) Task: {task.Name}({task.Stats.Active}) Connection '{connection?.Name}'({task.SQL.ConnectionId})");
                    if (!string.IsNullOrWhiteSpace(task.SQL.Command))
                    {
                        Console.WriteLine(task.SQL.Command);
                    }
                    var encCommand = server.Decrypt(task.SQL.EncryptedCommand);
                    if (!string.IsNullOrWhiteSpace(encCommand))
                    {
                        Console.WriteLine(encCommand);
                    }
                }
            }
        }
        private static Client GetClient()
        {
            Client.EventOnConnectedStatic += delegate (Client client1, ref Server server1)
            {
                Console.Error.WriteLine($"Connected to {server1.Connection.Address}");
            };
            Client.EventOnDisconnectStatic += delegate (ref Client client1, ref Server server1, bool exception, string error, Exception exception1)
            {
                Console.Error.WriteLine($"Disconnected from {server1.Connection.Address}. With error: {error}");
            };
            Client.EventOnConnectionProgress += delegate (Client client1, ref Server server1, int done, string messageStatus)
            {
                Console.Error.WriteLine($"Connecting to {server1.Connection.Address} [{done}] {messageStatus}");
            };
            Client.EventCommandNotUnderstood += delegate
            {
                Console.Error.WriteLine("Command not understood.");
            };
            var client = new Client();
            Console.WriteLine($"Client version: {client.GetFullVersion()}");
            return client;
        }

        private static Server ConnectToServer(Client client, bool isProd, string address = null)
        {
            var conn = new Connection
            {
                Address = address ?? (isProd ? "10.2.100.68" : "10.2.101.68"),
                UserName = "userName",
                PassWord = isProd ? "pasword" : "pasword",
                ConnectionType = Connection.ConnectionT.Remote,
                TimeOut = 600,
                UseCompression = true
            };

            var server = client.Connect(conn);
            Console.WriteLine($"Server ({server.Connection.Address}) version: {server.Version}");
            return server;
        }
    }
}
