# Transporter
Transfer data between two devices via UDP protocol.
Передача небольшого объема данных между двумя клиентами по средствам UDP протокола.

 ## Структура библиотеки
 - {} Transporter
   - **Transporter** - Реализует управление UDP клиентом.
 - {} Transporter.Service
   - **Client** - Реализация UDP клиента.
   - **DataEventHandler** - Сигнатура метода события получения данных.
   - **Message** - Сообщение с командой, по средствам которого общаются клиенты.
   - **MessageCommands** - Содержит команды для удаленного клиента.
   - **Metadata** - Метаданные о передаваемых данных.
   - **RConfig** - Содержит конфигурацию клиента.

## Подключение библиотеки
Для подключение библиотеки в VisualStudio необходимо:
1. Перейти в Менеджер ссылок (Проект -> Добавить ссылку). Нажать на кнопку "Обзор..." и выбрать необходимую библиотеку (В нашем случае это "Transporter.dll").
2. Разрешить использование пространства имен TransporterLib.
    ```C#
    using TransporterLib;
    ```

## Использование библиотеки
### Инициализация объекта Transporter.
Для работы с библиотекой необходимо инициализировать объект типа Transporter воспользовавшись одним из доступных конструкторов.
1. Для локальной работы.
    ```C#
    // Transporter(bool isSource)
    Transporter demoTransporter = Transporter(true);
    ```
    Параметр конструктора в значении true определяет, что объект будет настроен в качестве источника.
     ```C#
     // Transporter(bool isSource)
    Transporter demoTransporter = Transporter(false);
    ```
    Параметр конструктора в значении false определяет, что объект будет настроен в качестве получателя.
    Данный параметр необходим для корректной работы на одном устройстве двух клиентов.
2. для работы в локальной сети.
    Можно воспользоваться следующими конструкторами:
    ```C#
    // Transporter(string sourceClientIP, string destinationClientIP)
    Transporter demoTransporter = Transporter("192.168.1.1", "192.168.1.2");
    
    // или
    // public Transporter(IPAddress sourceClientIP, IPAddress destinationClientIP)
    IPAddress SourceIP = IPAddress.Parse("192.168.1.1");
    IPAddress DestinationIP = IPAddress.Parse("192.168.1.2");
    Transporter demoTransporter = Transporter(SourceIP, DestinationIP);
    ```
### Подписка на события и создание обработчиков событий.
Библиотека имеет несколько событий, которые можно использовать в своих целях.
Следующие события доступны для использования:
 - **onSClientGetData** - Возникает когда клиент получает данный от удаленного клиента.
    Сигнатура события - DataEventHandler(object data).
 - **onClientError** - Возникает в случае перехвата ошибки, произошедшей в клиенте.
    Сигнатура события - EventHandler<Exception>(object sender, TEventArgs e).
 - **onSClientDataListenerCreated** - Возникает когда клиент создает "Слушатель данных".
    Сигнатура события - EventHandler(object sender, EventArgs e).
 - **onSClientDataListenerClosed** - Возникает когда клиент закрыл "Слушатель данных".
    Сигнатура события - EventHandler(object sender, EventArgs e).
 - **onSClientMessageListenerCreated** - Возникает когда клиент создает "Слушатель сообщений".
    Сигнатура события - EventHandler(object sender, EventArgs e).
 - **onSClientMessageListenerClosed** - Возникает когда клиент закрыл "Слушатель сообщений".
    Сигнатура события - EventHandler(object sender, EventArgs e).
 - **onDClientDataListenerCreated** - Возникает когда удаленный клиент создал слушатель данных и готов к приему данных.
    Сигнатура события - EventHandler(object sender, EventArgs e).
 - **onDClientCancel** - Возникает когда удаленный клиент отверг операцию.
    Сигнатура события - EventHandler(object sender, EventArgs e).

Для полноценной работы с библиотекой требуется реализовать обработчик события onSClientGetData. Данный обработчик должен произвести дальнейшую обработку данных.

Пример:
``` C#
SomeClass
{
    private Transporter demoTransporter; // Объявляем объект.
    
    // Конструктор класса
    public SomeClass(bool isSource)
    {
        demoTransporter = new Transporter(isSource); // Инициализируем класс для работы в локальном режиме.
        
        // Подписываем на событие обработчик.
        if (isSource)
        {
            // Если источник, обрабатываем данные как источник.
            demoTransporter.onSClientGetData += sTransporter_onGetData; 
        }
        else
        {
            // Если удаленный клиент, обрабатываем данные как удаленный.
            demoTransporter.onSClientGetData += dTransporter_onGetData; 
        }
    }
    
    private void sTransporter_onGetData(object data)
    {
        // Код обработки полученных данных "data" как клиент-источник.
    }
    
    private void dTransporter_onGetData(object data)
    {
        // Код обработки полученных данных "data" как клиент-получатель.
    }
}
```
Аналогично оформляются другие события.

### Запуск прослушки сообщений.
Для того чтобы два клиента могли коммуницировать друг с другом, необходимо запустить "Слушатель сообщений (Message listener)".
Данный сервис будет прослушивать команды от других клиентов и реагировать в соответствии с полученными командами.
Чтобы запустить прослушку сообщений достаточно воспользоваться методом StartService();

Пример:
``` C#
SomeClass
{
    private Transporter demoTransporter; // Объявляем объект.
    
    public SomeClass(bool isSource)
    {
        demoTransporter = new Transporter(isSource); // Инициализируем класс для работы в локальном режиме.
        
        // Подписываем на событие обработчик.
        if (isSource) // Если источник, обрабатываем данные как источник.
            demoTransporter.onSClientGetData += sTransporter_onGetData; 
        else // Если удаленный клиент, обрабатываем данные как удаленный.
            demoTransporter.onSClientGetData += dTransporter_onGetData;
            
        // Запускаем прослушку сообщений
        demoTransporter.StartService();
    }
    
    private void sTransporter_onGetData(object data) {    }

    private void dTransporter_onGetData(object data) {    }
}
```

### Отправка данных.
Для отправки данных в классе Transporter предназначен метод SendObject(object obj).

Пример:
``` C#
SomeClass
{
    private Transporter demoTransporter; // Объявляем объект.
    
    public SomeClass(bool isSource)
    {
        demoTransporter = new Transporter(isSource); // Инициализируем класс для работы в локальном режиме.
        
        // Подписываем на событие обработчик.
        if (isSource) // Если источник, обрабатываем данные как источник.
            demoTransporter.onSClientGetData += sTransporter_onGetData; 
        else // Если удаленный клиент, обрабатываем данные как удаленный.
            demoTransporter.onSClientGetData += dTransporter_onGetData;
            
        // Запускаем прослушку сообщений
        demoTransporter.StartService();
    }
    
    Private void SendPI()
    {
        double decPI = Math.PI; // Тестовые данные для отправки.
        demoTransporter.SendObject(decPI); // Отправили объект типа double содержащий число PI.
    }
    
    private void sTransporter_onGetData(object data)
    {
        double decPIxPI = (double)data; // Полученные данные назначаем переменной.
    }

    private void dTransporter_onGetData(object data)
    {
        double decPI = (double)data; // Преобразовали data к виду если это возможно.
        double decPIxPI = Math.Pow(decPI,2); // Возводим в квадрат.
        demoTransporter.SendObject(decPIxPI); // Отправляем обратно.
    }
}
```

### Остановка прослушки сообщений.
Для завершения работы "Слушателя сообщений" предусмотрен метод StopService().

Пример:
``` C#
SomeClass
{
    private Transporter demoTransporter; // Объявляем объект.
    
    public SomeClass(bool isSource)
    {
        demoTransporter = new Transporter(isSource); // Инициализируем класс для работы в локальном режиме.
        
        // Подписываем на событие обработчик.
        if (isSource) // Если источник, обрабатываем данные как источник.
            demoTransporter.onSClientGetData += sTransporter_onGetData; 
        else // Если удаленный клиент, обрабатываем данные как удаленный.
            demoTransporter.onSClientGetData += dTransporter_onGetData;
            
        // Запускаем прослушку сообщений
        demoTransporter.StartService();
    }
    
    Private void SendPI()
    {
        double decPI = Math.PI; // Тестовые данные для отправки.
        demoTransporter.SendObject(decPI); // Отправили объект типа double содержащий число PI.
    }
    
    private void sTransporter_onGetData(object data)
    {
        double decPIxPI = (double)data; // Полученные данные назначаем переменной.
        // После получения данных обратно можно прекратить работу по прослушке сообщений.
        demoTransporter.StopService();
    }

    private void dTransporter_onGetData(object data)
    {
        double decPI = (double)data; // Преобразовали data к виду если это возможно.
        double decPIxPI = Math.Pow(decPI,2); // Возводим в квадрат.
        demoTransporter.SendObject(decPIxPI); // Отправляем обратно.
    }
}
```
### Замена конфигурационных данных.
Иногда, требуется изменить адрес удаленного клиента или другие конфигурационные данные без создания нового экземпляра класса Transporter.
Для таких случаев предназначен метод SetConfig(RConfig config). Он принимает в качестве единственного параметра  экземпляр класса RConfig, содержащий новые данные конфигурации.

Пример использования замены конфигурационных данных представлен ниже:
``` C#
SomeClass
{
    private Transporter demoTransporter; // Объявляем объект.
    
    public SomeClass(bool isSource) //Инициализирует локальный клиент
    {
        demoTransporter = new Transporter(isSource); // Инициализируем класс для работы в локальном режиме.
        ...
        // Локальная конфигурация клиента в зависимости от параметра конструктора.
        RConfig newConfig = new RConfig(isSource); 
        demoTransporter.SetConfig(newConfig); // Обновили конфигурации в соответствии с новыми параметрами.
        ...
    }
    
    public SomeClass(bool isSource, string sourceIP , string destinationIP) / /Инициализирует клиент для работы по сети
    {
        demoTransporter = new Transporter(isSource);
        ...
        // Локальная конфигурация клиента в зависимости от параметра конструктора.
        RConfig newConfig = new RConfig(sourceIP, destinationIP); // Указали IP источника и получателя.
        demoTransporter.SetConfig(newConfig); // Обновили конфигурации в соответствии с новыми параметрами.
        ...
    }
    
    Private void SendPI()    {...}
    private void sTransporter_onGetData(object data)    {...}
    private void dTransporter_onGetData(object data)    {...}
}
```
## Примеры.
Ниже предоставлены несколько примеров:
 - **[Demonstration]** - Демонстрация возможностей библиотеки. В качестве данных использует файлы небольшого объема.
 - **SimpleCalc** - Показывает базовый пример использования на примере вычисления небольшого математического выражения.
    - [SimpleCalcSource] - Приложение-источник.
    - [SimpleCalcDestination] - Приложение-получатель данных.

## Используемые источники.
 - **[metanit_SocUDP]** - Использование сокетов для работы с UDP.
 - **[wikipedia_UDP]** - User Datagram Protocol.
 - **[stackoverflow_GetLocIP]** - Get local IP address.

[//]: # (Список ссылок)

[Demonstration]: <https://github.com/IlyaKrotkikh/Transporter/tree/master/Demonstration>
[SimpleCalcDestination]: <https://github.com/IlyaKrotkikh/Transporter/tree/master/SimpleCalcDestination>
[SimpleCalcSource]: <https://github.com/IlyaKrotkikh/Transporter/tree/master/SimpleCalcSource>

[metanit_SocUDP]: <https://metanit.com/sharp/net/3.3.php>
[wikipedia_UDP]: <https://en.wikipedia.org/wiki/User_Datagram_Protocol>
[stackoverflow_GetLocIP]: <https://stackoverflow.com/questions/6803073/get-local-ip-address>



