using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using WcfFileService;
namespace FileServiceClient
{
    class ClientSession
    {
        private static string userName;
        public static string User
        {
            get { return userName; }
            set { userName = value; }
        }
    }
    class NotifyCallback : IFileServiceCallback
    {
        public void Notify(string notify)
        {
            string user = notify.Split(' ')[0];
            if (!user.Equals(ClientSession.User) && !notify.Split(' ')[1].Equals(""))
            {
                Console.WriteLine(notify);
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please connect to service.");
            string cmd = Console.ReadLine();
            bool connected = false;
            IFileService client = null;
            while (!connected && !cmd.ToLower().Equals("quit"))
            {
                string command = cmd.Split(' ')[0];
                if (command.ToLower().Equals("connect") && cmd.Split(' ').Count() > 2)
                {
                    string serviceHost = cmd.Split(' ')[1];
                    var binding = new WSDualHttpBinding();
                    var address = new EndpointAddress($"http://{serviceHost}/FileService.svc");
                    NotifyCallback notifyCallback = new NotifyCallback();
                    InstanceContext instance = new InstanceContext(notifyCallback);
                    var factory = new DuplexChannelFactory<IFileService>(notifyCallback, binding, address);
                    client = factory.CreateChannel();
                    String answer = client.RunCommand(cmd);
                    if (answer.ToLower().Contains("error"))
                    {
                        Console.WriteLine(answer);
                        cmd = Console.ReadLine();
                    }
                    else
                    {
                        ClientSession.User = answer;
                        connected = true;
                    }
                    
                }
                else
                {
                    Console.WriteLine("You are not connected! Please connect!");
                    cmd = Console.ReadLine();
                }
                
            }
            if  (cmd.ToLower() != "quit")
            {
                Console.WriteLine("Welcome! You are connected!");
                Console.WriteLine(client.CurrentPath());
                cmd = Console.ReadLine();
                while (!cmd.ToLower().Equals("quit"))
                {
                    if (client != null)
                    {
                        String answer = client.RunCommand(cmd);
                        Console.WriteLine(answer);
                        Console.WriteLine(client.CurrentPath());
                    }
                    cmd = Console.ReadLine();
                }
                if (client != null)
                {
                    client.Logout();
                }
            }
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }
    }
}
