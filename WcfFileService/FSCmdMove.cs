using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;

namespace WcfFileService
{
    //Реализация команды MOVE
    public class FSCmdMove : IFileServiceCommand
    {
        private string source;
        private string desination;
        public string Notify(params string[] args)
        {
            return $"moved {source} to {desination}";
        }

        public string Run(string sessionId, params string[] args)
        {
            string filename = args[0].Split('\\')[args[0].Split('\\').Count() - 1];
            string path = args[0].Replace(filename, "");
            source = args[0];
            desination = args[1];
            Monitor.Enter(VirtualFileSystem.Lock);
            if (filename.Contains(".txt"))
            {
                string result = VirtualFileSystem.MoveFile(sessionId, filename, path, args[1]);
                Monitor.Exit(VirtualFileSystem.Lock);
                return result;
            }
            else
            {
                string result = VirtualFileSystem.MoveDir(sessionId, args[0], args[1]);
                Monitor.Exit(VirtualFileSystem.Lock);
                return result;
            }
            

        }
    }
}