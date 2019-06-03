using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDFileServiceLibrary
{
    public class NotifyList
    {
        private static Dictionary<String, String> fNotifyList = new Dictionary<String, String>() {
            {"connect", "WcfFileService.FSCmdConnect"}
        };
        public static String Get(String notify)
        {
            return fNotifyList[notify];
        }
    }
    class FSNotifyFactory
    {
        public static IFSNotify NewNotify(string notify)
        {
            IFSNotify result = null;
            Type fsNotifyType = Type.GetType(NotifyList.Get(notify), false, true);
            if (fsNotifyType != null)
            {
                System.Reflection.ConstructorInfo ci = fsNotifyType.GetConstructor(new Type[] { });
                result = (IFSNotify)ci.Invoke(new object[] { });
            }
            else
            {
                throw new FileServiceCommandExeption($"\"{notify}\" not found");
            }
            return result;
        }
    }
}
