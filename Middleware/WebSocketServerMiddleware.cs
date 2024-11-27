using System.Net.WebSockets;
using System.Text;
using System.Linq;
using System.Text.Json;
using WebSocketServer.Models;
using Server.Models;
using Microsoft.AspNetCore.Http;


namespace WebSocketServer.Middleware;

public static class WebSocketServerMiddlewareExtensions 
{
    public static IApplicationBuilder UseWebSocketServer(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<WebSocketServerMiddleware>();
    }
} 

public static class SocketExtensions
{
    public async static Task SendTextMessageAsync(this WebSocket socket, string message)
    {
        await socket.SendAsync(
            Encoding.UTF8.GetBytes(message),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    } 
}

public class WebSocketServerMiddleware
{
    private readonly RequestDelegate _next;

    private readonly WebSocketConnectionManager _manager;

    public WebSocketServerMiddleware(
        RequestDelegate next,
        WebSocketConnectionManager manager)
    {
        _next = next;
        _manager = manager;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        WriteRequestParam(context);

        if (context.WebSockets.IsWebSocketRequest)
        {
            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
            string connId = _manager.AddSocket(webSocket);

            // Определение типа клиента на основе порта
            int port = context.Connection.LocalPort;
            //int clientType = (port == 5000) ? 1 : 2; // Например: порт 5000 для Type 1, другой для Type 2
            
            if (!File.Exists("Clients.json"))
            {
                File.WriteAllText("Clients.json", "[]");
            }

            await RecieveMessage(webSocket, async (result, buffer) =>
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await RouteJsonMessageAsync(message, connId, webSocket, port);
                    return;
                }
                else if (result.MessageType == WebSocketMessageType.Close
                         && result.CloseStatus != null
                         && _manager.TryRemoveSocket(connId, out WebSocket? removedSocket))
                {
                    Console.WriteLine("Received Close message");
                    await removedSocket.CloseAsync(
                        result.CloseStatus.Value,
                        result.CloseStatusDescription,
                        CancellationToken.None);
                    return;
                }
            });
        }
        else
        {
            Console.WriteLine("Hello from 2nd request delegate!");
            await _next(context);
        }
    }

    private async Task SendConnIdAsync(WebSocket socket, string connId)
    {
        var buffer = Encoding.UTF8.GetBytes("ConnId: "+ connId);
        await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private void WriteRequestParam (HttpContext context) 
    {
        Console.WriteLine("Request Method: " + context.Request.Method);
        Console.WriteLine("Request Protocol: " + context.Request.Protocol);

        if(context.Request.Headers != null)
        {
            foreach(var h in context.Request.Headers)
            {
                Console.WriteLine("--> " + h.Key + " : " + h.Value);
            }
        }
    }

    private async static Task RecieveMessage(
        WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
    {
        var buffer = new byte[1024 *64];

        while(socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(
                buffer: new ArraySegment<byte>(buffer),
                CancellationToken.None);
                 
            handleMessage(result, buffer);
        }
    }
    private async Task SendPathsToClient(WebSocket webSocket, List<string> paths)
    {
        if (webSocket != null && webSocket.State == WebSocketState.Open)
        {
            var json = JsonSerializer.Serialize(paths);
            var buffer = Encoding.UTF8.GetBytes(json);
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
    public async Task RouteJsonMessageAsync(string messageInput, string connId, WebSocket socket, int port)
    {
        List<string> paths = new List<string>();
        List<ClientLevel> clients = new List<ClientLevel>();

        // Чтение файла clients.json
        if (File.Exists("Clients.json"))
        {
            string jsonContent = File.ReadAllText("Clients.json");
            clients = JsonSerializer.Deserialize<List<ClientLevel>>(jsonContent) ?? new List<ClientLevel>();
        }

        // Десериализация сообщения от клиента
        var client = JsonSerializer.Deserialize<ClientLevel>(messageInput)!;

        // Проверка, существует ли клиент
        var existingClient = clients.Find(c => c.ClientName == client.ClientName);

        if (existingClient == null)
        {
            // Новый клиент — добавляем его в список
            clients.Add(client);
            File.WriteAllText("Clients.json", JsonSerializer.Serialize(clients));
        }
        else
        {
            // Обновление данных существующего клиента
            existingClient.Days = client.Days;
            File.WriteAllText("Clients.json", JsonSerializer.Serialize(clients));
        }

        // Создание файла paths для текущего клиента
        //if (!File.Exists(connId + "_paths.json"))
        //{
        //    foreach (var data in client.Days[^1].Data)
        //    {
        //        paths.Add(data.Path);
        //    }
        //    File.WriteAllText(connId + "_paths.json", JsonSerializer.Serialize(paths));
        //    await SendPathsToClient(socket, paths);
        //}

        // Обработка типов клиентов
        //await HandleClientMessage(clientType, socket);
    }
}
//serialize list client day