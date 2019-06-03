using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace MDFileServiceLibrary
{
    [ServiceContract(CallbackContract = typeof(IFileServiceCallback))]
    public interface IFileService
    {
        [OperationContract]
        String TestConnection();
        [OperationContract]
        String RunCommand(String cmd);
    }

    public interface IFileServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void Notify(string notify);

    }


}
