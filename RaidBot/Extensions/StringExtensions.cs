namespace T.Extensions
{
    public static class StringExtensions
    {
        public static string TrimString(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return value.Trim();
        }
    }
}