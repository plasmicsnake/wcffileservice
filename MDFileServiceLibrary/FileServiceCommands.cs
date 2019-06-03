using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MDFileServiceLibrary
{
    public class CommadList
    {
        private static Dictionary<String, String> fCommandList = new Dictionary<String, String>() {
            {"connect", "MDFileServiceLibrary.FSCmdConnect"}
        };
        public static String Get(String cmd)
        {
            return fCommandList[cmd];
        }
    }
    public class FileServiceCommands
    {
        public static IFileServiceCommand NewCommand(string cmd)
        {
            IFileServiceCommand result = null;
            Type fsCommandType = Type.GetType(CommadList.Get(cmd), false, true);
            if (fsCommandType != null)
            {
                System.Reflection.ConstructorInfo ci = fsCommandType.GetConstructor(new Type[] { });
                result = (IFileServiceCommand)ci.Invoke(new object[] { });
            }
            else
            {
                throw new FileServiceCommandExeption($"Commnand \"{cmd}\" not found");
            }
            return result;
        }
    }
}