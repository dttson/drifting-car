using System;

namespace Utils
{
    public static class NumberExtensions
    {
        public static string FormatCoin(this int coinAmount)
        {
            return $"{coinAmount:N0}";
        }
        
        public static string FormatTimeFromSecondsPretty(this int totalSeconds)
        {
            if (totalSeconds < 0)
            {
                throw new ArgumentException("Seconds cannot be negative.");
            }

            if (totalSeconds == 0)
                return "--:--:--";

            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;

            string result;
            
            if (hours > 0)
                result = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
            else if (minutes > 0)
                result = $"--:{minutes:D2}:{seconds:D2}";
            else
                result = $"--:--:{seconds:D2}";

            return result;
        }
        
        public static string FormatTimeFromSeconds(this int totalSeconds)
        {
            if (totalSeconds < 0)
            {
                throw new ArgumentException("Seconds cannot be negative.");
            }

            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;

            string result;
            
            if (hours > 0)
                result = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
            else if (minutes > 0)
                result = $"{minutes:D2}:{seconds:D2}";
            else
                result = $"{seconds:D2}";

            return result;
        }
    }
}