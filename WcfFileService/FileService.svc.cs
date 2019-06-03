using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;

namespace WcfFileService
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени класса "Service1" в коде, SVC-файле и файле конфигурации.
    // ПРИМЕЧАНИЕ. Чтобы запустить клиент проверки WCF для тестирования службы, выберите элементы Service1.svc или Service1.svc.cs в обозревателе решений и начните отладку.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class FileService : IFileService
    {

        public string CurrentPath()
        {
            return VirtualFileSystem.GetPath(OperationContext.Current.SessionId);
        }

        public void Logout()
        {
            FileServiceSession.DeleteUser(OperationContext.Current.SessionId);
        }

        public string RunCommand(string cmd)
        {
            string fsCommand = cmd.Split(' ')[0];
            string[] args = cmd.Split(' ').Skip(1).ToArray();
            try
            {
                IFileServiceCommand fsCommandObj = FileServiceCommands.NewCommand(fsCommand.ToLower());
                string answer = fsCommandObj.Run(OperationContext.Current.SessionId, args);
                OperationContext.Current.GetCallbackChannel<IFileServiceCallback>().Notify(FileServiceSession.GetUser(OperationContext.Current.SessionId) + " " + fsCommandObj.Notify(args));
                return answer;
            }
            catch (FileServiceCommandExeption e)
            {
                return $"Error: {e.Message}";
            }
            finally
            {
                if (Monitor.IsEntered(VirtualFileSystem.Lock))
                {
                    Monitor.Exit(VirtualFileSystem.Lock);
                }
            }
        }

        public string TestConnection()
        {
            return "OK";
        }
    }
}
