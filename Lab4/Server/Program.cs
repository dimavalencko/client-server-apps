using System.Net;
using System.Net.Sockets;
using System.Text;

var server = new TcpListener(IPAddress.Any, 8888);
server.Start();
Console.WriteLine($"Сервер запущен. Ожидание подключений на порту {8888}...");

try
{
    while (true)
    {
        var client = await server.AcceptTcpClientAsync();
        Console.WriteLine($"\nКлиент подключен: {client.Client.RemoteEndPoint}");

        // Запускаем обработку клиента в отдельном потоке без ожидания
        _ = HandleClientAsync(client);
    }
}
finally
{
    server.Stop();
}

async Task HandleClientAsync(TcpClient client)
{
    try
    {
        using (client)
        using (var stream = client.GetStream())
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer);

            if (bytesRead == 0)
            {
                Console.WriteLine("Клиент отключился.");
                return;
            }

            string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Получено от {client.Client.RemoteEndPoint}: {request}");

            // Проверяем, что ввод — это числа
            if (!request.Split(',').All(x => int.TryParse(x.Trim(), out _)))
            {
                byte[] error = Encoding.UTF8.GetBytes("Ошибка: введите только числа!");
                await stream.WriteAsync(error);
                return;
            }

            // Считаем сумму
            var numbers = request.Split(',').Select(int.Parse);
            int sum = numbers.Sum();

            // Отправляем ответ клиенту
            byte[] response = Encoding.UTF8.GetBytes(sum.ToString());
            await stream.WriteAsync(response);
            Console.WriteLine($"Отправлено клиенту {client.Client.RemoteEndPoint}: {sum}");
        }
    }
    catch (IOException ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Клиент отключился: {ex.Message}");
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Ошибка: {ex.Message}");
        Console.ResetColor();
    }
}