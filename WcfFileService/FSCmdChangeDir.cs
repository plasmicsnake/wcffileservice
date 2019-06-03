using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WcfFileService
{
    //Реализация команды CD
    public class FSCmdChangeDir : IFileServiceCommand
    {
        public string Notify(params string[] args)
        {
            return "";
        }

        public string Run(string sessionId, params string[] args)
        {
            VirtualFileSystem.SetPath(sessionId, args[0]);
            return "";
        }
    }
}