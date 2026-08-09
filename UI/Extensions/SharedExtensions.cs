namespace UI.Extensions
{
    public static class SharedExtensions
    {
        public static string GetDisplayClass(this decimal amount) => amount >= 0 ? "text-success" : "text-danger";

        public static decimal GetDisplaySpent(this decimal amount) => Math.Abs(amount);
    }
}