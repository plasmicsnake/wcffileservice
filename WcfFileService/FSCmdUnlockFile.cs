﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WcfFileService
{
    //Реализация команды UNLOCK
    public class FSCmdUnlockFile : IFileServiceCommand
    {
        private string filename;
        public string Notify(params string[] args)
        {
            return $"unlocked file {filename}";
        }

        public string Run(string sessionId, params string[] args)
        {
            string result = " unlocked";
            string file = args[0].Split('\\')[args[0].Split('\\').Count() - 1];
            string path = args[0].Replace(file, "");
            filename = VirtualFileSystem.UnlockFile(sessionId, path, file);
            result = filename + result;
            return result;
        }
    }
}