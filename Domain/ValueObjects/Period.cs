namespace Domain.ValueObjects
{
    public class Period
    {
        public int Month { get; private set; }
        public int Year { get; private set; }

        public Period(int month, int year)
        {
            if (month < 1 || month > 12)
                throw new ArgumentException("Month must be between 1 and 12", nameof(month));

            if (year < 2000 || year > 2100)
                throw new ArgumentException("Invalid year", nameof(year));

            Month = month;
            Year = year;
        }

        public DateTime ToDateTime() => new DateTime(Year, Month, 1);

        public bool IsSameMonth(Period other) =>
            Month == other.Month && Year == other.Year;

        public bool IsSameMonth(DateTime date) =>
            Month == date.Month && Year == date.Year;
    }
}