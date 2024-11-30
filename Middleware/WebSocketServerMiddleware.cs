using Server.Models;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using WebSocketServer.Models;
using System.Text.Json;
using System.Timers;
using System.Threading;

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
                            Console.WriteLine("www");
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
                    clients = ClientLevel.AddOrUpdateClientLevel(clients, client); /////////
                    ClientLevel.WriteClientLevelsToFile(clients, "Clients.json");
                    _stoper = false;
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
    private static void OnTimedEvent(Object source, ElapsedEventArgs e)
    {
        _stoper = !_stoper; // Переключение значения
        //Console.WriteLine($"Значение myBoolValue: {_stoper}"); // Вывод текущего значения
    }
    private async Task HandleClientType2(WebSocket webSocket)
    {
        List<ClientLevel> clients = new List<ClientLevel>();
        
        // Логика обработки для клиентов на порту 6000
        while (webSocket.State == WebSocketState.Open)
        {
            string message = await SocketExtensions.ReceiveTextMessageAsync(webSocket);
            
            if (File.Exists("Clients.json"))
            {
                clients = ClientLevel.ReadClientLevelsFromFile("Clients.json");                
                if (message == "admin here")
                {
                    Console.WriteLine("admin logged");
                    await SocketExtensions.SendClientLevelsAsync(webSocket, clients);
                    await Task.Delay(1000);

                }
                if (message == "Update")
                {
                    Console.WriteLine("Update");
                    _stoper = true;
                    while (true)
                    {
                        if (!_stoper)
                        {
                            Console.WriteLine("Updating");
                            await SocketExtensions.SendClientLevelsAsync(webSocket, clients);
                            await Task.Delay(1000);
                            break;
                        }
                        
                    } 
                }
                if (message.StartsWith("Set-Timer "))
                {
                    string number = message.Replace("Set-Timer ", "");

                    Storage.time =Convert.ToInt32(number) * 60 *1000;
                    var  timer = new System.Timers.Timer(Storage.time);
                    timer.Elapsed += OnTimedEvent;
                    timer.AutoReset = true; // Включаем повторение
                    timer.Enabled = true; // Запускаем таймер

                }
                if (message.StartsWith("remove from monitor"))
                {
                    string path = message.Replace("remove from monitor ", "");
                    foreach (var client in clients)
                    {
                        client.Days[^1].Data.RemoveAll(data => data.Path == path);
                    }
                    File.Delete("Clients.json");
                    ClientLevel.WriteClientLevelsToFile(clients, "Clients.json");
                    await SocketExtensions.SendClientLevelsAsync(webSocket, clients);
                    Console.WriteLine("remove from monitor");
                    await Task.Delay(1000);
                    continue;
                }
                if (message.StartsWith("add to monitor"))
                {
                    string path = message.Replace("add to monitor ", "");
                    Storage.path = path;
                    _stoper = true;
                    while (true)
                    {
                        if (!_stoper)
                        {
                            clients = ClientLevel.ReadClientLevelsFromFile("Clients.json");
                            await SocketExtensions.SendClientLevelsAsync(webSocket, clients);
                            
                            
                            Storage.path = "";
                            await Task.Delay(100);

                            break;
                        }
                    }
                    continue;
                }
                if (message.StartsWith("remove from register"))
                {
                    
                    Console.WriteLine("remove from register");
                    await Task.Delay(1000);
                    continue;
                }
                if (message.StartsWith("add to register"))
                {
                    Console.WriteLine("333");
                    await Task.Delay(1000);
                    continue;
                }

                else
                {
                    Console.WriteLine("4");
                    await Task.Delay(1000);
                    continue;
                }
            }
            await Task.Delay(5000); // Отправка каждые 5 секунд
        }
    }

    private async Task SendMessage(WebSocket socket, string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}
