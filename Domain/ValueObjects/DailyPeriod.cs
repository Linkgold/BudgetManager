namespace Domain.ValueObjects
{
    public class DailyPeriod
    {
        private const int JANUARY = 1;
        private const int DECEMBER = 12;

        public int Day { get; private set; }
        public int Month { get; private set; }
        public int Year { get; private set; }

        public DailyPeriod(int day, int month, int year)
        {
            if (month < JANUARY || month > DECEMBER) throw new ArgumentException("Month must be between 1 and 12", nameof(month));

            if (year < 1900 || year > 2100) throw new ArgumentException("Year must be between 1900 and 2100", nameof(year));

            if (day < 1 || day > DateTime.DaysInMonth(year, month)) throw new ArgumentException($"Day must be between 1 and {DateTime.DaysInMonth(year, month)}", nameof(day));

            Day = day;
            Month = month;
            Year = year;
        }

        public DateTime ToDateTime() => new DateTime(Year, Month, Day);

        public MonthlyPeriod ToMonthlyPeriod() => new MonthlyPeriod(Month, Year);

        public bool IsSameDay(DailyPeriod other) => (other is null) ? false : Day == other.Day && Month == other.Month && Year == other.Year;

        public bool IsSameMonth(DailyPeriod other) => (other is null) ? false : Month == other.Month && Year == other.Year;

        public bool IsSameMonth(MonthlyPeriod date) => (date is null) ? false : Month == date.Month && Year == date.Year;

        public DailyPeriod NextDay()
        {
            DateTime next = ToDateTime().AddDays(1);
            return new DailyPeriod(next.Day, next.Month, next.Year);
        }

        public DailyPeriod PreviousDay()
        {
            DateTime previous = ToDateTime().AddDays(-1);
            return new DailyPeriod(previous.Day, previous.Month, previous.Year);
        }

        public bool IsWeekend()
        {
            return ToDateTime().DayOfWeek == DayOfWeek.Saturday ||
                   ToDateTime().DayOfWeek == DayOfWeek.Sunday;
        }

        public DailyPeriod NextMonth()
        {
            int nextMonth = Month == DECEMBER ? JANUARY : Month + 1;
            int nextYear = Month == DECEMBER ? Year + 1 : Year;

            // 🔥 Validar que el día existe en el nuevo mes
            int validDay = Math.Min(Day, DateTime.DaysInMonth(nextYear, nextMonth));

            if (Month == DECEMBER) return new DailyPeriod(Day, JANUARY, Year + 1);
            return new DailyPeriod(validDay, Month + 1, Year);
        }

        public DailyPeriod PreviousMonth()
        {
            int previousMonth = Month == JANUARY ? DECEMBER : Month - 1;
            int previousYear = Month == JANUARY ? Year - 1 : Year;

            // 🔥 1. Obtener el día del mes anterior (si existe)
            int dayInPreviousMonth = Math.Min(Day, DateTime.DaysInMonth(previousYear, previousMonth));

            // 🔥 Validar que el día existe en el nuevo mes
            int validDay = Math.Min(dayInPreviousMonth, DateTime.DaysInMonth(previousYear, previousMonth));

            if (Month == JANUARY) return new DailyPeriod(Day, DECEMBER, Year - 1);
            return new DailyPeriod(validDay, Month - 1, Year);
        }

        public bool Equals(DailyPeriod other)
        {
            if (other is null) return false;
            return Day == other.Day && Year == other.Year && Month == other.Month;
        }

        public override bool Equals(object obj) => Equals(obj as DailyPeriod);

        public override int GetHashCode() => HashCode.Combine(Day, Year, Month);

        public override string ToString() => $"{Day:00}/{Month:00}/{Year:0000}";

        public static bool operator ==(DailyPeriod a, DailyPeriod b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(DailyPeriod a, DailyPeriod b) => !(a == b);
    }
}