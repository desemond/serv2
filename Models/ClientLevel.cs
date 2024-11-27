namespace Server.Models
{
    public class ClientLevel
    {
        public string ClientName { get; set; }
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
                //newClientLevel==existingClient
                // Объединить данные
                foreach (var newDay in newClientLevel.Days)
                {
                    // Найти существующий DayLevel с той же датой
                    var existingDay = existingClient.Days.FirstOrDefault(d => d.Day == newDay.Day);

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
    }
}
