using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MDFileServiceLibrary
{
    public class FileServiceSession
    {
        private static Dictionary<string, string> session = new Dictionary<string, string>();
        public static String GetUser(string sessionId)
        {
            return (from u in session where u.Value.Equals(sessionId) select u.Key).First();
        }

        public static void SetUser(string sessionId, string user)
        {
            if ((from u in session where u.Key.Equals(user) select u.Key).Count() == 0)
            {
                session.Add(user, sessionId);
            }
            else
            {
                throw new FileServiceCommandExeption("User already connected");
            }
        }
    }
}