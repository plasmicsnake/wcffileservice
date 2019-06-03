using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace MDFileServiceLibrary
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class FileService : IFileService
    {
        private IFileServiceCallback Callback
        {
            get
            {
                return OperationContext.Current.GetCallbackChannel<IFileServiceCallback>();
            }
        }
        public string RunCommand(string cmd)
        {
            string fsCommand = cmd.Split(' ')[0];
            string[] args = new string[] { };
            cmd.Split(' ').CopyTo(args, 1);
            try
            {
                string answer = FileServiceCommands.NewCommand(fsCommand.ToLower()).Run(OperationContext.Current.SessionId, args);
                Callback.Notify(FSNotifyFactory.NewNotify(FileServiceSession.GetUser(OperationContext.Current.SessionId) + " " + fsCommand.ToLower()).GetNotify());
                return answer;
            }
            catch (FileServiceCommandExeption e)
            {
                return $"Error: {e.Message}";
            }
        }

        public string TestConnection()
        {
            throw new NotImplementedException();
        }
    }
}
