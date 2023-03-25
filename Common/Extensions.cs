using System;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

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

        public static string RemoveIllegalCharacters(this string text)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars());
            Regex r = new Regex($"[{Regex.Escape(regexSearch)}]");
            text = r.Replace(text, "");
            return text;
        }
        public static string RemoveIllegalCharactersFromPath(this string text)
        {
            string regexSearch = new string(Path.GetInvalidPathChars()) + ":?";
            Regex r = new Regex($"[{Regex.Escape(regexSearch)}]");
            text = r.Replace(text, "");
            return text;
        }

        public static Bitmap ResizeImage(this Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static byte[] ImageToByte2(this Image img)
        {
            using var stream = new MemoryStream();
            img.Save(stream, ImageFormat.Jpeg);
            return stream.ToArray();
        }
    }
}