using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDFileServiceLibrary
{
    public interface IFileServiceCommand
    {
        String Run(string sessionId, params String[] args);
    }
}
