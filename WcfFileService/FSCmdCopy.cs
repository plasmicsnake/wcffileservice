using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace WcfFileService
{
    //Реализация команды COPY
    public class FSCmdCopy : IFileServiceCommand
    {
        private string source;
        private string desination;
        private readonly object vfsLock = new object();
        public string Notify(params string[] args)
        {
            return $"copied {source} to {desination}";
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
                return VirtualFileSystem.CopyFile(sessionId, filename, path, args[1]);
            }
            else
            {
                return VirtualFileSystem.CopyDir(sessionId, args[0], args[1]);
            }
            Monitor.Exit(VirtualFileSystem.Lock);

        }
    }
}