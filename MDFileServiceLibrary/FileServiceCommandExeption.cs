using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MDFileServiceLibrary
{
    public class FileServiceCommandExeption : Exception
    {
        public FileServiceCommandExeption(string msg) :base(msg)
        {

        }
    }
}