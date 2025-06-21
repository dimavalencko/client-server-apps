using System.Net.Sockets;
using System.Text;
using System.Threading;
class Program
{
    private const int delayMs = 10000;
    private const string ServerIp = "127.0.0.1";
    private const int ServerPort = 1234;
    private static readonly string StudentData = $"ФИО: Валенко Дмитрий Владимирович, группа: ПИН-Б-З-22-1, Вариант - 1";
    private static int retryCount = 0;
    static async Task Main()
    {
        Console.WriteLine("Клиент запущен. Для выхода нажмите Ctrl+C...");

        while (true)
        {
            try
            {
                await ConnectAndSendDataAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine($"Повторное подключение через {delayMs / 1000} секунд...");
                Thread.Sleep(delayMs); // Пауза перед повторной попыткой
            }
            retryCount++;
        }
    }

    private static async Task ConnectAndSendDataAsync()
    {
        using var client = new TcpClient();

        try
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Тест - {retryCount + 1}");
            Console.ResetColor();
            Console.WriteLine($"Подключение к серверу {ServerIp}:{ServerPort}...");
            await client.ConnectAsync(ServerIp, ServerPort);

            if (!client.Connected)
                throw new Exception("Не удалось подключиться к серверу.");

            Console.WriteLine("Подключение успешно!");

            using var stream = client.GetStream();
            byte[] data = Encoding.UTF8.GetBytes(StudentData);

            // Отправка данных
            await stream.WriteAsync(data, 0, data.Length);
            Console.WriteLine("Данные отправлены!");

            // Получение ответа (макс. 1 КБ)
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

            if (bytesRead == 0)
                throw new Exception("Сервер закрыл соединение.");

            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Ответ сервера: {response}");
            await Task.Delay(5000);
        }
        catch (SocketException ex) // Обработка ошибок сети
        {
            throw new Exception($"Ошибка сети: {ex.Message}");
        }
        catch (IOException ex) // Обработка ошибок чтения/записи
        {
            throw new Exception($"Ошибка передачи данных: {ex.Message}");
        }
        finally
        {
            client.Close();
        }
    }
}
