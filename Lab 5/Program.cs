using System.Net;

class Program
{
    static async Task Main(string[] args)
    {
        // Создаем сервер и определяем путь к папке www
        var server = new HttpServer("http://127.0.0.1:8080/", Path.Combine(Directory.GetCurrentDirectory(), "www"));
        await server.StartAsync(); // Запускаем сервер асинхронно
    }
}