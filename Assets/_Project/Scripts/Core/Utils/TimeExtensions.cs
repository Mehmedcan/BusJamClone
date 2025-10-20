using UnityEngine;

namespace _Project.Scripts.Core.Utils
{
    public static class TimeExtensions
    {
        public static string ToMinuteSecond(this int totalSeconds)
        {
            var total = Mathf.FloorToInt(totalSeconds);
            var minutes = total / 60;
            var seconds = total % 60;
            return $"{minutes:D2}:{seconds:D2}";
        }
    }
}