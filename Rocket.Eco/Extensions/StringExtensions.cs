namespace System
{
    public static class StringExtensions
    {
        public static bool ComparerContains(this string text, string value, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase) => text.IndexOf(value, stringComparison) >= 0;
    }
}