using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;

namespace WcfFileService
{
    //Реализация команды DELTREE
    public class FSCmdDeleteTree : IFileServiceCommand
    {
        private string directory = "";
        public string Notify(params string[] args)
        {
            return $"deleted \"{directory}\" with subdirectories";
        }

        public string Run(string sessionId, params string[] args)
        {
            if (args.Count() > 0)
            {
                Monitor.Enter(VirtualFileSystem.Lock);
                directory = VirtualFileSystem.DeleteTreeDirectory(sessionId, args[0]);
                Monitor.Exit(VirtualFileSystem.Lock);
                return $"{directory} deleted";
            }
            else
            {
                throw new FileServiceCommandExeption("Missed directiory name");
            }
        }
    }
}