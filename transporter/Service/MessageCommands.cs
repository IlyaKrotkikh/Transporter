using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Команды для удаленного клиента.
/// </summary>
namespace Transporter.Service
{
    public enum MessageCommands
    {
        OpenDataListener, // Открыть слушатель данных.
        CloseMessageListener, // Закрыть слушатель сообщений.
        DataListenerCreated, // Слушатель данных был создан.
        IsFree, // Удаленный клиент свободен?
        OK, // OK
        Cancel // Отмена/Ошибка операции
    }
}
