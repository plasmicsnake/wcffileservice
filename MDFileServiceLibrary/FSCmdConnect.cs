using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MDFileServiceLibrary
{
    public class FSCmdConnect : IFileServiceCommand
    {
        public string Run(string sessionId, params string[] args)
        {
            if (args.Count() > 0)
            {
                FileServiceSession.SetUser(sessionId, args[0]);
                return args[0];
            }
            else
            {
                throw new FileServiceCommandExeption("Missed user name");
            }
        }
    }
}