# Transporter
Transfer data between two devices via UDP protocol
Передача небольшого объема данных между двумя клиентами по средствам udp протокола.
 ## Структура библиотеки
 - {} Transporter
   - **Transporter** - Реализует управление UDP клиентом.
 - {} Transporter.Service
   - **Client** - Реализация UDP клиента.
   - **DataEventHandler** - Сигнатура метода события получения данных.
   - **Message** - Сообщение с командой, по средствам которого общаются клиенты.
   - **MessageCommands** - Содержит команды для удаленного клиента.
   - **Metadata** - Метаданные о предаваемых данных.
   - **RConfig** - Содержит конфигурацию клиента.
