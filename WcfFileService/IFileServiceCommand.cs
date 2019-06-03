using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WcfFileService
{
    //Интерфейс реализации команд сервиса
    public interface IFileServiceCommand
    {
        //Выполнить команду
        String Run(string sessionId, params String[] args);
        //Получить уведомение
        String Notify(params String[] args);
    }
}
