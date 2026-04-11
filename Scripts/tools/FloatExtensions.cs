using I2.Loc;
using UnityEngine;

namespace tools
{
    public static class FloatExtensions
    {
        public static string FormatTime(this float time)
        {
            int totalSeconds = Mathf.FloorToInt(time);

            int days = totalSeconds / 86400;
            totalSeconds %= 86400;

            int hours = totalSeconds / 3600;
            totalSeconds %= 3600;

            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            if (days > 0)
                return $"{days}{LocalizationManager.GetTranslation("Game/d")} {hours}{LocalizationManager.GetTranslation("Game/h")}";

            if (hours > 0)
                return $"{hours}{LocalizationManager.GetTranslation("Game/h")} {minutes}{LocalizationManager.GetTranslation("Game/m")}";

            if (minutes > 0)
                return $"{minutes}{LocalizationManager.GetTranslation("Game/m")} {seconds}{LocalizationManager.GetTranslation("Game/s")}";

            return $"{seconds}{LocalizationManager.GetTranslation("Game/s")}";
        }

    }
}