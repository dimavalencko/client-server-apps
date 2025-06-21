using System.Net;
using System.Net.Sockets;
using System.Text;
using Server;
using Newtonsoft.Json;

var server = new TcpListener(IPAddress.Any, 8888);
server.Start();
Console.WriteLine($"Сервер запущен. Ожидание подключений на порту {8888}...");

try
{
    while (true)
    {
        var client = await server.AcceptTcpClientAsync();
        Console.WriteLine($"\nКлиент подключен: {client.Client.RemoteEndPoint}");
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
            // Получаем длину данных
            byte[] lengthBuffer = new byte[4];
            await stream.ReadAsync(lengthBuffer);
            int requestLength = BitConverter.ToInt32(lengthBuffer);

            // Получаем сами данные
            byte[] requestBuffer = new byte[requestLength];
            await stream.ReadAsync(requestBuffer);
            string jsonRequest = Encoding.UTF8.GetString(requestBuffer);
            Console.WriteLine(jsonRequest);
            var request = JsonConvert.DeserializeObject<NumberRequest>(jsonRequest);

            Console.WriteLine($"Получено от {client.Client.RemoteEndPoint}: {string.Join(", ", request?.Numbers ?? new List<int>())}");

            var response = new NumberResponse();

            if (request?.Numbers == null || request.Numbers.Count == 0)
            {
                response.Error = "Не получены числа для обработки";
            }
            else
            {
                response.Sum = request.Numbers.Sum();
            }

            // Сериализуем и отправляем ответ
            string jsonResponse = JsonConvert.SerializeObject(response);
            byte[] responseData = Encoding.UTF8.GetBytes(jsonResponse);

            // Отправляем длину данных
            await stream.WriteAsync(BitConverter.GetBytes(responseData.Length));
            // Отправляем сами данные
            await stream.WriteAsync(responseData);

            Console.WriteLine($"Отправлено клиенту {client.Client.RemoteEndPoint}: Сумма = {response.Sum}");
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