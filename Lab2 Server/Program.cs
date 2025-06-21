using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Введите порт для сервера (например, 8888):");
        if (!int.TryParse(Console.ReadLine(), out int port))
        {
            Console.WriteLine("Некорректный порт. Используется порт по умолчанию 8888.");
            port = 8888;
        }

        var server = new TcpListener(IPAddress.Any, port);
        server.Start();
        Console.WriteLine($"Сервер запущен на порту {port}. Ожидание подключений...");

        try
        {
            while (true)
            {
                // Принимаем клиента асинхронно
                TcpClient client = await server.AcceptTcpClientAsync();
                Console.WriteLine($"\nКлиент подключен: {client.Client.RemoteEndPoint}");

                // Обработка клиента в отдельном таске (чтобы не блокировать прием новых)
                _ = Task.Run(() => HandleClientAsync(client));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка сервера: {ex.Message}");
        }
        finally
        {
            server.Stop();
        }
    }

    private static async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            using (client)
            using (NetworkStream stream = client.GetStream())
            {
                byte[] buffer = new byte[1024]; // Буфер для данных (1 КБ)
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                {
                    Console.WriteLine("Клиент отключился, не отправив данных.");
                    return;
                }

                // Получение и вывод данных от клиента
                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Получены данные: {request}");

                // Отправка ответа (эхо-ответ)
                byte[] responseData = Encoding.UTF8.GetBytes($"Сервер получил: {request}");
                await stream.WriteAsync(responseData, 0, responseData.Length);
                Console.WriteLine("Ответ отправлен.");
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Клиент разорвал соединение: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка обработки клиента: {ex.Message}");
        }
    }
}