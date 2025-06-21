using System.Net;

// Класс HttpServer нужен для создания сервера и прослушивания обращений
public class HttpServer : IDisposable
{
    private readonly HttpListener _listener;
    private readonly string _wwwRoot;
    private readonly RequestHandler _handler;

    public HttpServer(string url, string wwwRoot)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add(url);
        _wwwRoot = wwwRoot;
        _handler = new RequestHandler(_wwwRoot);
    }

    public async Task StartAsync()
    {
        _listener.Start(); // Стартуем сервер
        Console.WriteLine($"Сервер запущен на {string.Join(", ", _listener.Prefixes)}");

        // Слушаем обращения по переданному в конструктор адресу
        while (true)
        {
            var context = await _listener.GetContextAsync();
            _ = _handler.ProcessRequestAsync(context); // Передаем каждый запрос в RequestHandler не блокируя основной поток
        }
    }

    // Обязательно чистим мусор
    public void Dispose()
    {
        _listener.Stop();
        _listener.Close();
    }
}