using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WcfFileService
{
    //Обработка исключений в сервисе
    public class FileServiceCommandExeption : Exception
    {
        public FileServiceCommandExeption(string msg) :base(msg)
        {

        }
    }
}