namespace Domain.ValueObjects
{
    public class Period
    {
        public int Month { get; private set; }
        public int Year { get; private set; }

        public Period(int month, int year)
        {
            if (month < 1 || month > 12) throw new ArgumentException("Month must be between 1 and 12", nameof(month));

            if (year < 1900 || year > 2100) throw new ArgumentException("Invalid year", nameof(year));

            Month = month;
            Year = year;
        }

        public DateTime ToDateTime() => new DateTime(Year, Month, 1);

        public bool IsSameMonth(Period other) => Month == other.Month && Year == other.Year;

        public bool IsSameMonth(DateTime date) => Month == date.Month && Year == date.Year;

        public Period NextMonth()
        {
            if (Month == 12) return new Period(Year + 1, 1);
            return new Period(Year, Month + 1);
        }

        public Period PreviousMonth()
        {
            if (Month == 1) return new Period(Year - 1, 12);
            return new Period(Year, Month - 1);
        }

        public bool Equals(Period other)
        {
            if (other is null) return false;
            return Year == other.Year && Month == other.Month;
        }

        public override bool Equals(object obj) => Equals(obj as Period);

        public override int GetHashCode() => HashCode.Combine(Year, Month);

        public override string ToString() => $"{Year:0000}-{Month:00}";

        public static bool operator ==(Period a, Period b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(Period a, Period b) => !(a == b);
    }
}