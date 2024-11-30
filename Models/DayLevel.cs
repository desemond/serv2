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

        public static List<DayLevel> AddOrUpdateDayLevel(List<DayLevel> dayLevels, DayLevel newDayLevel)
        {
            // Найти существующий DayLevel
            var existingDay = dayLevels.FirstOrDefault(c => c.Day == newDayLevel.Day);

            if (existingDay != null)
            {
                // Обновляем данные существующего DayLevel
                foreach (var newData in newDayLevel.Data)
                {
                    var existingData = existingDay.Data.FirstOrDefault(d => d.Path == newData.Path);

                    if (existingData != null)
                    {
                        // Обновляем существующий DataLevel
                        existingData.UpdateFrom(newData);
                    }
                    else
                    {
                        // Выравниваем новый DataLevel
                        AlignDataLevel(existingDay, newData);
                        Console.WriteLine("sssssss");
                        // Добавляем новый DataLevel
                        existingDay.Data.Add(newData);
                        
                    }
                }
            }
            else
            {
                // Добавляем новый DayLevel
                dayLevels.Add(newDayLevel);
            }

            // Возвращаем обновленный список
            return dayLevels;
        }


        private static void AlignDataLevel(DayLevel existingDay, DataLevel newData)
        {
            // Выравниваем Status, CheckTime и LastWriteTime, если требуется
            if (newData.Status.Count < existingDay.Data[0].Status.Count)
            {
                int missingCount = existingDay.Data[0].Status.Count - newData.Status.Count;

                for (int i = 0; i < missingCount; i++)
                {
                    newData.Status.Insert(0, false);
                    newData.CheckTime.Insert(0, DateTime.MinValue);
                }
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