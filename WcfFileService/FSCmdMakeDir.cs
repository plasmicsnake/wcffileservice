using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;

namespace WcfFileService
{
    //Реализация команды MD
    public class FSCmdMakeDir : IFileServiceCommand
    {
        private string directory = "";
        public string Notify(params string[] args)
        {
            return $"make directory {directory}";
        }

        public string Run(string sessionId, params string[] args)
        {
            if (args.Count() > 0)
            {
                Monitor.Enter(VirtualFileSystem.Lock);
                directory = VirtualFileSystem.CreateDirectory(sessionId, args[0]);
                Monitor.Exit(VirtualFileSystem.Lock);
                return $"{directory} created";
            }
            else
            {
                throw new FileServiceCommandExeption("Missed directiory name");
            }
        }
    }
}