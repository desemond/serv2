using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Server.Models
{
    public class ClientLevel
    {
        public string ClientName { get; set; }
        public string connId { get; set; }
        public List<DayLevel> Days { get; set; }

        public ClientLevel()
        {
            this.ClientName = "client1";
            this.Days = new List<DayLevel>();
        }

        public ClientLevel(string name, List<DayLevel> days)
        {
            this.ClientName = name;
            this.Days = days;
        }
        public static List<ClientLevel> AddOrUpdateClientLevel(List<ClientLevel> clientLevels, ClientLevel newClientLevel)
        {
            // Найти существующего клиента
            var existingClient = clientLevels.FirstOrDefault(c => c.ClientName == newClientLevel.ClientName);

            if (existingClient != null)
            {
                // Обновляем существующего клиента
                foreach (var newDay in newClientLevel.Days)
                {
                    // Найти существующий DayLevel
                    var existingDay = existingClient.Days.FirstOrDefault(d => d.Day == newDay.Day);

                    if (existingDay != null)
                    {
                        // Обновляем существующий DayLevel и получаем обновленный список
                        existingClient.Days = DayLevel.AddOrUpdateDayLevel(existingClient.Days, newDay);
                        foreach (var data in existingClient.Days[^1].Data)
                        {
                            Console.WriteLine(data.Path);
                        }
                    }
                    else
                    {
                        // Добавляем новый DayLevel
                        existingClient.Days.Add(newDay);
                    }
                }
            }
            else
            {
                // Добавляем нового клиента
                clientLevels.Add(newClientLevel);
            }

            // Возвращаем обновленный список клиентов
            return clientLevels;
        }




        public static List<string> getPaths(ClientLevel client)
        {
            List<string> paths = new List<string>();

            foreach ( var data in client.Days[^1].Data)
            {
                paths.Add(data.Path);
            }
            return paths;
        }
        public static void WriteClientLevelToFile(ClientLevel clientLevel, string filePath)
        {
            string json = JsonSerializer.Serialize(clientLevel);
            File.WriteAllText(filePath, json);
        }
        public static ClientLevel ReadClientLevelFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<ClientLevel>(json)
                   ?? throw new JsonException("Deserialization of ClientLevel failed.");
            }
            else
            {
                throw new FileNotFoundException($"File {filePath} not found.");
            }
        }
        public static void WriteClientLevelsToFile(List<ClientLevel> clientLevels, string filePath)
        {
            string json = JsonSerializer.Serialize(clientLevels);
            File.WriteAllText(filePath, json);
        }
        public static List<ClientLevel> ReadClientLevelsFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<List<ClientLevel>>(json)
            ?? throw new JsonException("Deserialization of List<ClientLevel> failed.");
            }
            else
            {
                throw new FileNotFoundException($"File {filePath} not found.");
            }
        }
    }
}
