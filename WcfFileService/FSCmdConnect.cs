using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WcfFileService
{
    //Реализация команды CONNECT
    public class FSCmdConnect : IFileServiceCommand
    {
        public string Notify(params string[] args)
        {
            return "connected";
        }

        public string Run(string sessionId, params string[] args)
        {
            if (args.Count() > 1)
            {
                FileServiceSession.SetUser(sessionId, args[1]);
                VirtualFileSystem.SetPath(sessionId, "C:");
                return FileServiceSession.GetUser(sessionId);
            }
            else
            {
                throw new FileServiceCommandExeption("Missed user name");
            }
        }
    }
}