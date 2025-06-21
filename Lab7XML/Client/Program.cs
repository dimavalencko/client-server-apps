using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;
using Client;

while (true)
{
    Console.WriteLine("\nВведите числа через запятую (или 'exit' для выхода):");
    string input = Console.ReadLine() ?? "";

    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    if (!input.Split(',').All(x => int.TryParse(x.Trim(), out _)))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Ошибка: введите только числа, разделенные запятыми!");
        Console.ResetColor();
        continue;
    }

    using var client = new TcpClient();

    try
    {
        await client.ConnectAsync("127.0.0.1", 8888);
        Console.WriteLine("Подключено к серверу.");

        using var stream = client.GetStream();

        // Создаем и сериализуем запрос в XML
        var request = new NumberRequest
        {
            Numbers = input.Split(',').Select(x => int.Parse(x.Trim())).ToList()
        };

        var xmlSerializer = new XmlSerializer(typeof(NumberRequest));
        using var textWriter = new StringWriter();
        xmlSerializer.Serialize(textWriter, request);
        string xmlRequest = textWriter.ToString();

        byte[] requestData = Encoding.UTF8.GetBytes(xmlRequest);

        Console.WriteLine("XML запрос к серверу");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(xmlRequest);
        Console.ResetColor();

        // Отправляем длину данных
        await stream.WriteAsync(BitConverter.GetBytes(requestData.Length));
        // Отправляем сами данные
        await stream.WriteAsync(requestData);

        // Получаем ответ
        byte[] lengthBuffer = new byte[4];
        await stream.ReadAsync(lengthBuffer);
        int responseLength = BitConverter.ToInt32(lengthBuffer);

        byte[] responseBuffer = new byte[responseLength];
        await stream.ReadAsync(responseBuffer);
        string xmlResponse = Encoding.UTF8.GetString(responseBuffer);
        Console.WriteLine("XML ответ от сервера:");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(xmlResponse);
        Console.ResetColor();

        // Десериализуем ответ
        var responseSerializer = new XmlSerializer(typeof(NumberResponse));
        using var textReader = new StringReader(xmlResponse);
        var response = (NumberResponse)responseSerializer.Deserialize(textReader);

        if (!string.IsNullOrEmpty(response?.Error))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Ошибка: {response.Error}");
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine($"Сумма чисел: {response?.Sum}");
        }
    }
    catch (SocketException ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Ошибка подключения: {ex.Message}");
        Console.WriteLine("Повторная попытка через 3 секунды...");
        await Task.Delay(3000);
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Ошибка: {ex.Message}");
        Console.ResetColor();
    }
}