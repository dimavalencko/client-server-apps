using System.Net;
using System.Security;
using System.Text;

public class RequestHandler
{
    private readonly string _wwwRoot;
    private readonly string _defaultPage = "index.html";
    private readonly string _errorPage = "error-page.html";

    public RequestHandler(string wwwRoot)
    {
        _wwwRoot = wwwRoot;
    }

    /// <summary>
    /// Метод для обработки запросов и формировании ответов
    /// </summary>
    /// <param name="context"></param>
    public async Task ProcessRequestAsync(HttpListenerContext context)
    {
        try
        {
            var request = context.Request;
            var response = context.Response;

            string filePath = GetFilePath(request.Url!.AbsolutePath);

            if (File.Exists(filePath))
            {
                await SendFileAsync(response, filePath);
            }
            else
            {
                await SendErrorPageAsync(response, 404, "Not Found");
            }
        }
        catch (Exception ex)
        {
            await SendErrorPageAsync(context.Response, 500, $"Internal Server Error: {ex.Message}");
        }
        finally
        {
            context.Response.Close();
        }
    }

    /// <summary>
    /// Метод преобразования URL в путь к файлу
    /// </summary>
    /// <param name="context"></param>
    private string GetFilePath(string urlPath)
    {
        string safePath = urlPath.Trim('/');
        if (string.IsNullOrEmpty(safePath)) safePath = _defaultPage;

        // Удаляем .html если есть
        safePath = safePath.Replace(".html", "");

        // Пробуем найти файл с расширением .html
        string filePath = Path.Combine(_wwwRoot, $"{safePath}.html");

        // Проверка безопасности пути
        if (!filePath.StartsWith(_wwwRoot))
        {
            throw new SecurityException("Attempted path traversal");
        }

        return filePath;
    }

    /// <summary>
    /// Метод асинхронного чтения файла и отправки ответа клиенту
    /// </summary>
    /// <param name="context"></param>
    private async Task SendFileAsync(HttpListenerResponse response, string filePath)
    {
        byte[] content = await File.ReadAllBytesAsync(filePath);
        response.ContentType = GetContentType(filePath);
        response.ContentLength64 = content.Length;
        await response.OutputStream.WriteAsync(content);
    }

    /// <summary>
    /// Метод отправки страницы с ошибкой
    /// </summary>
    /// <param name="context"></param>
    private async Task SendErrorPageAsync(HttpListenerResponse response, int statusCode, string statusText)
    {
        response.StatusCode = statusCode;
        string errorPagePath = Path.Combine(_wwwRoot, _errorPage);

        if (File.Exists(errorPagePath))
        {
            await SendFileAsync(response, errorPagePath);
        }
        else
        {
            byte[] errorMsg = Encoding.UTF8.GetBytes($"{statusCode} {statusText}");
            response.ContentType = "text/plain";
            await response.OutputStream.WriteAsync(errorMsg);
        }
    }

    /// <summary>
    /// Метод получения типа контента по пути
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    private string GetContentType(string filePath)
    {
        return Path.GetExtension(filePath).ToLower() switch
        {
            ".html" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            _ => "application/octet-stream",
        };
    }
}