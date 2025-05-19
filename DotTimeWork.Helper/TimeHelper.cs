namespace DotTimeWork.Helper
{
    public static class TimeHelper
    {
        public static string GetWorkingTimeHumanReadable(TimeSpan timeSpan)
        {
            return GetWorkingTimeHumanReadable((int)timeSpan.TotalMinutes);
        }

        public static string GetWorkingTimeHumanReadable(int minutes)
        {
            if (minutes < 60)
            {
                return $"{minutes} minutes";
            }
            else if (minutes < 1440)
            {
                return $"{minutes / 60} hours";
            }
            else
            {
                int rest = minutes % 1440;
                return $"{minutes / 1440} days and "+(rest/60)+" hours";
            }
        }
    }
}
