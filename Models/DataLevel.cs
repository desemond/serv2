using Microsoft.AspNetCore.Antiforgery;

namespace Server.Models
{
    public class DataLevel
    {
        public string Path { get; set; }
        public List<string> Size { get; set; }
        public string Type { get; set; }
        public int Quantity { get; set; }
        public List<bool> Status { get; set; }
        public List<DateTime> CheckTime { get; set; }
        public List<DateTime> LastWriteTime { get; set; }
        public Dictionary<string, object> RegistryValues { get; set; }

        public DataLevel(string path)
        {
            Size = new List<string>();
            Status = new List<bool>();
            CheckTime = new List<DateTime>();
            LastWriteTime = new List<DateTime>();
            RegistryValues = new Dictionary<string, object>();

            Path = path;
            CheckTime.Add(DateTime.Now);
        }

        public void UpdateFrom(DataLevel other)
        {
            // Добавляем новое время проверки
            CheckTime.Add(DateTime.Now);

            // Сравниваем размер
            if (Size.Count > 0 && other.Size.Count > 0 && Size[^1] == other.Size[^1])
            {
                Status.Add(true);
            }
            else
            {
                if (other.Size.Count > 0)
                    Size.Add(other.Size[^1]);
                Status.Add(false);
            }

            // Сравниваем время последнего изменения
            if (LastWriteTime.Count > 0 && other.LastWriteTime.Count > 0 && LastWriteTime[^1] == other.LastWriteTime[^1])
            {
                Status[^1] = Status[^1] && true;
            }
            else
            {
                if (other.LastWriteTime.Count > 0)
                    LastWriteTime.Add(other.LastWriteTime[^1]);
                Status[^1] = false;
            }

            // Сравниваем значения в реестре
            if (RegistryValues.Count == other.RegistryValues.Count)
            {
                bool registryMatch = true;
                foreach (var kvp in RegistryValues)
                {
                    if (!other.RegistryValues.TryGetValue(kvp.Key, out var otherValue) || !Equals(kvp.Value, otherValue))
                    {
                        registryMatch = false;
                        break;
                    }
                }
                if (registryMatch)
                {
                    Status[^1] = Status[^1] && true;
                }
                else
                {
                    RegistryValues = new Dictionary<string, object>(other.RegistryValues);
                    Status[^1] = false;
                }
            }
            else
            {
                RegistryValues = new Dictionary<string, object>(other.RegistryValues);
                Status[^1] = false;
            }
        }
    }

}
