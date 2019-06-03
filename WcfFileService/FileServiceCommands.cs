using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WcfFileService
{
    //Список команд поддерживаемых сервисом
    public class CommadList
    {
        private static Dictionary<String, String> fCommandList = new Dictionary<String, String>() {
            {"connect", "WcfFileService.FSCmdConnect"},
            {"md", "WcfFileService.FSCmdMakeDir"},
            {"cd", "WcfFileService.FSCmdChangeDir"},
            {"print", "WcfFileService.FSCmdPrint"},
            {"rd", "WcfFileService.FSCmdDeleteDir"},
            {"mf", "WcfFileService.FSCmdMakeFile"},
            {"del", "WcfFileService.FSCmdDeleteFile"},
            {"deltree", "WcfFileService.FSCmdDeleteTree"},
            {"lock", "WcfFileService.FSCmdLockFile"},
            {"unlock", "WcfFileService.FSCmdUnlockFile"},
            {"move", "WcfFileService.FSCmdMove"},
            {"copy", "WcfFileService.FSCmdCopy"}

        };
        public static String Get(String cmd)
        {
            if (fCommandList.ContainsKey(cmd))
            {
                return fCommandList[cmd];
            }
            else
            {
                throw new FileServiceCommandExeption($"Commnand \"{cmd}\" not found");
            }

        }
    }
    //Фабрика по производству команд
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