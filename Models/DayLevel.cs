namespace Server.Models
{
    public class DayLevel
    {
        public DateOnly Day { get; set; }
        public List<DataLevel> Data { get; set; }
        public DayLevel()
        {
            List<DataLevel> Data = new List<DataLevel>();
        }
        public DayLevel(List<DataLevel> data)
        {
            this.Day = DateOnly.FromDateTime(DateTime.Now);
            this.Data = data;
        }

        public static void AddOrUpdateDayLevel(List<DayLevel> dayLevels, DayLevel newDayLevel)
        {
            // Найти существующего клиента с таким же именем
            var existingDay = dayLevels.FirstOrDefault(c => c.Day == newDayLevel.Day);
            //existingDay==newDayLevel
            if (existingDay != null)
            {
                // Объединить данные
                foreach (var newDay in newDayLevel.Data)
                {
                    // Найти существующий DayLevel с той же датой
                    var existingData = existingDay.Data.FirstOrDefault(d => d.Path == newDay.Path);

                    if (existingData != null)
                    {
                        existingData.UpdateFrom(newDay);
                    }
                    else
                    {
                        // Добавить новый DayLevel
                        existingDay.Data.Add(newDay);
                    }
                }
            }
            else
            {
                // Если клиент не найден, добавляем его в список
                dayLevels.Add(newDayLevel);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is DayLevel other)
            {
                return Day.Equals(other.Day) && Data.SequenceEqual(other.Data);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Day, Data);
        }
    }
}