using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;

namespace WcfFileService
{
    //Реализация команды MF
    public class FSCmdMakeFile : IFileServiceCommand
    {
        private string filename;
        public string Notify(params string[] args)
        {
            return $"create file {filename}";
        }

        public string Run(string sessionId, params string[] args)
        {
            string result = " created";
            string file = args[0].Split('\\')[args[0].Split('\\').Count() - 1];
            string path = args[0].Replace(file, "");
            Monitor.Enter(VirtualFileSystem.Lock);
            filename = VirtualFileSystem.CreateFile(sessionId, path, file);
            Monitor.Exit(VirtualFileSystem.Lock);
            result = filename + result;
            return result;
        }
    }
}