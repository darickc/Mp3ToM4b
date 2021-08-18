using System;
using System.IO;

namespace Mp3ToM4b.Common
{
    public static class Extensions
    {
        public static TimeSpan ToTimeSpan(this string time)
        {
            var pieces = time.Split(':');
            if (pieces.Length == 2)
            {
                var minutes = int.Parse(pieces[0]);
                var s = pieces[1].Split('.');

                var seconds = int.Parse(s[0]);
                var ms = int.Parse(s[1]);
                return new TimeSpan(0, 0, minutes, seconds, ms);
            }
            if (pieces.Length == 3)
            {
                var hours = int.Parse(pieces[0]);
                var minutes = int.Parse(pieces[1]);
                var s = pieces[2].Split('.');

                var seconds = int.Parse(s[0]);
                var ms = int.Parse(s[1]);
                return new TimeSpan(0, hours, minutes, seconds, ms);
            }
            return TimeSpan.Parse(time);
        }

        public static bool NotEmpty(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        public static string RemoveInvalidChars(this string filename)
        {
            return string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
        }

        public static string EscapeName(this string value)
        {
            value = value.Replace("'", "'\\''");
            return value;
        }
    }
}