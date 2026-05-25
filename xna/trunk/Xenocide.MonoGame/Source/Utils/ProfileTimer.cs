using System;
using System.Diagnostics;

namespace ProjectXenocide.Utils
{
    public static class Profile
    {
        public static IDisposable Time(string label)
        {
#if DEBUG
            return new ProfileScope(label);
#else
            return null;
#endif
        }

        [Conditional("DEBUG")]
        public static void Log(string label, long elapsedMs)
        {
            Console.WriteLine($"[PROFILE] {label}: {elapsedMs}ms");
        }

        private class ProfileScope : IDisposable
        {
            private readonly string _label;
            private readonly Stopwatch _sw;

            public ProfileScope(string label)
            {
                _label = label;
                _sw = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                _sw.Stop();
                Log(_label, _sw.ElapsedMilliseconds);
            }
        }
    }
}
