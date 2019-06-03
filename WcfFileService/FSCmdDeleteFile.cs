using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WcfFileService
{
    //Реализация команды DEL
    class FSCmdDeleteFile : IFileServiceCommand
    {
        private string filename;
        public string Notify(params string[] args)
        {
            return $"deleted file \"{filename}\"";
        }

        public string Run(string sessionId, params string[] args)
        {
            string result = " deleted";
            string file = args[0].Split('\\')[args[0].Split('\\').Count() - 1];
            string path = args[0].Replace(file, "");
            Monitor.Enter(VirtualFileSystem.Lock);
            filename = VirtualFileSystem.DeleteFile(sessionId, path, file);
            Monitor.Exit(VirtualFileSystem.Lock);
            result = filename + result;
            return result;
        }
    }
}
