using System.Globalization;

namespace ProjectXenocide.Utils
{
    public static class GameCulture
    {
        public static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        public static string Format(int value) => value.ToString(Culture);
        public static string Format(uint value) => value.ToString(Culture);
        public static string Format(long value) => value.ToString(Culture);
        public static string Format(float value) => value.ToString(Culture);
        public static string Format(double value) => value.ToString(Culture);

        public static System.StringComparer Comparer => System.StringComparer.Ordinal;
        public static System.StringComparison Comparison => System.StringComparison.Ordinal;
    }
}
