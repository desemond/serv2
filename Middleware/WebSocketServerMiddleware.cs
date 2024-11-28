using Server.Models;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using WebSocketServer.Models;
using System.Text.Json;

public class WebSocketMiddleware
{
    private static bool _stoper = false;
    private readonly RequestDelegate _next;

    public WebSocketMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
            int port = context.Connection.LocalPort;

            // Определяем тип клиента по порту
            if (port == 5000)
            {
                await HandleClientType1(webSocket);
            }
            else if (port == 6000)
            {
                await HandleClientType2(webSocket);
            }
            else
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Unknown port", CancellationToken.None);
            }
        }
        else
        {
            await _next(context);
        }
    }
    private static async Task WaitForStoperAsync()
    {
        while (!_stoper)
        {
            await Task.Delay(100); // Check every 100 milliseconds
        }
    }
    private async Task HandleClientType1(WebSocket webSocket)
    {
        List<ClientLevel> clients = new List<ClientLevel>();
        ClientLevel client = new ClientLevel();

        await SocketExtensions.SendTextMessageAsync(webSocket, "Are you there?");
        string message = await SocketExtensions.ReceiveTextMessageAsync(webSocket);
        string ipPattern = @"\b(?:\d{1,3}\.){3}\d{1,3}\b";
        var match = Regex.Match(message, ipPattern);
        Console.WriteLine(message);

        // Логика обработки для клиентов на порту 5000
        while (webSocket.State == WebSocketState.Open)
        {
            

            if (match.Success)
            {
                // Если в сообщении найден IP-адрес
                string ipAddress = match.Value;
                if (message == ipAddress)
                {
                    
                    // Если сообщение состоит только из IP-адреса
                    if (File.Exists("Clients.json"))
                    {
                        clients = ClientLevel.ReadClientLevelsFromFile("Clients.json");
                        var existingClient = clients.FirstOrDefault(c => c.ClientName == ipAddress);
                        if (existingClient != null)
                        {
                            await SocketExtensions.SendPaths(webSocket, ClientLevel.getPaths(existingClient));
                        }
                        else
                        {
                            await SocketExtensions.SendTextMessageAsync(webSocket, "first run");
                        }
                    }
                    else
                    {
                        await SocketExtensions.SendTextMessageAsync(webSocket, "first run");
                    }

                    
                    client = await SocketExtensions.ReceiveClientLevelAsync(webSocket);
                    ClientLevel.AddOrUpdateClientLevel(clients, client); /////////
                    ClientLevel.WriteClientLevelsToFile(clients, "Clients.json");
                    await WaitForStoperAsync();
                }
                else
                {
                    // Если сообщение содержит IP и другие данные
                    //await HandleMessageWithIp(socket, message, ipAddress);
                }
            }
            else
            {
                // Если IP-адрес не найден
                //await HandleNonIpMessage(socket, message);
            }


            await Task.Delay(1000); // Отправка каждые 5 секунд
        }
    }

    private async Task HandleClientType2(WebSocket webSocket)
    {
        // Логика обработки для клиентов на порту 6000
        while (webSocket.State == WebSocketState.Open)
        {
            await SendMessage(webSocket, "Hello from port 6000!");
            await Task.Delay(5000); // Отправка каждые 5 секунд
        }
    }

    private async Task SendMessage(WebSocket socket, string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}
