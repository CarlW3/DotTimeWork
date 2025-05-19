namespace DotTimeWork.Helper
{
    public static class Guard
    {
        public static void AgainstNullOrEmpty(string? value, string paramName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"Argument {paramName} cannot be null or empty.", paramName);
            }
        }

        public static void AgainstNull(object? value, string paramName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName, $"Argument {paramName} cannot be null.");
            }
        }

        public static void AgainstNegative(int value, string paramName)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(paramName, $"Argument {paramName} cannot be negative.");
            }
        }

    }
}
