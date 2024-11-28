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
        public static void AddOrUpdateClientLevel(List<ClientLevel> clientLevels, ClientLevel newClientLevel)
        {
            // Найти существующего клиента с таким же именем
            var existingClient = clientLevels.FirstOrDefault(c => c.ClientName == newClientLevel.ClientName);
            
            if (existingClient != null)
            {
                Console.WriteLine(newClientLevel.Days[0]);
                //newClientLevel==existingClient
                // Объединить данные
                foreach (var newDay in newClientLevel.Days)
                {

                    
                    // Найти существующий DayLevel с той же датой
                    var existingDay = existingClient.Days.FirstOrDefault(d => d.Day == newDay.Day);
                     /// erorrrrrrrr
                    if (existingDay != null)
                    {
                        DayLevel.AddOrUpdateDayLevel(existingClient.Days, existingDay);

                    }
                    else
                    {
                        // Добавить новый DayLevel
                        existingClient.Days.Add(newDay);
                    }
                }
            }
            else
            {
                // Если клиент не найден, добавляем его в список
                clientLevels.Add(newClientLevel);
            }
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
