using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;

namespace WcfFileService
{
    //Контракт для работы с сервисом
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IFileServiceCallback))]
    public interface IFileService
    {
        //Проверка соединения
        [OperationContract]
        String TestConnection();
        //Выполнение команды
        [OperationContract]
        String RunCommand(String cmd);
        //Получение текущей директории, в которой находится пользователей
        [OperationContract]
        String CurrentPath();
        //Выход пользователя из сервиса
        [OperationContract]
        void Logout();

    }
    //Контракт для получения уведомлений
    public interface IFileServiceCallback
    {
        //Уведомление
        [OperationContract]
        void Notify(string notify);

    }

}
