using System.Net.WebSockets;
using System.Text;
using System.Linq;
using System.Text.Json;
using WebSocketServer.Models;
using Server.Models;
using Microsoft.AspNetCore.Http;
//using Newtonsoft.Json;

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
        if(context.WebSockets.IsWebSocketRequest)
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString();
            Console.WriteLine($"Client IP Address: {clientIp}");
            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
            Console.WriteLine("WebSocket Connected"); 
            string connId = _manager.AddSocket(webSocket);
            if (!File.Exists("Clients.json"))
            {
                await SocketExtensions.SendTextMessageAsync(webSocket, "first run");
            }
            else
            {
                string jsonString = File.ReadAllText("Clients.json");
                List<ClientLevel>  clients = new List<ClientLevel>();
                clients = JsonSerializer.Deserialize <List<ClientLevel>>(jsonString);
                var port = context.Connection.LocalPort;
                Console.WriteLine(port);
                if (port == 5000)
                {
                    await SocketExtensions.SendTextMessageAsync(webSocket, "first run");
                }
            }


            //await SendConnIdAsync(webSocket, connId);
            await RecieveMessage(webSocket, async (result, buffer) => 
            {
                if(result.MessageType == WebSocketMessageType.Text)
                {
                    Console.WriteLine("Message recieved");
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Message: {message}");
                    await RouteJsonMessageAsync(message, connId,webSocket);
                    return;
                }
                else if(result.MessageType == WebSocketMessageType.Close
                        && result.CloseStatus != null
                        && _manager.TryRemoveSocket(connId, out WebSocket? removedSocket))
                {
                    Console.WriteLine("Recieved Close message");
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
    public async Task RouteJsonMessageAsync(string messageInput,string connId, WebSocket socket)
    {
        List<string> paths= new List<string>();
        List<ClientLevel> clients = new List<ClientLevel>();
        var client = JsonSerializer.Deserialize<ClientLevel>(messageInput)!;
        if (!File.Exists("Clients.json"))
        {
            clients.Add(client);
            File.WriteAllText( "Clients.json", JsonSerializer.Serialize(client));
        }
        else
        {
            
        }
        if (!File.Exists(connId + "_paths.json"))
        {
            foreach (var data in client.Days[^1].Data)
            {
                paths.Add(data.Path);

            }
            File.WriteAllText(connId+ "paths.json", JsonSerializer.Serialize(paths));
            await SendPathsToClient(socket, paths);
        } 
    }
}