using System;
using System.Globalization;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using NLog;

namespace ProjectXenocide.Utils
{
    /// <summary>
    /// Captures the current game back buffer to a PNG.
    ///
    /// <para>
    /// Triggered in-game by the Print Screen key (with F12 as a fallback, since
    /// some Windows configurations reserve Print Screen for the OS snipping
    /// tool). Files are written to a <c>Screenshots</c> directory under the
    /// process working directory, named <c>yyyyMMdd-HHmmss-&lt;label&gt;.png</c>.
    /// </para>
    ///
    /// <para>
    /// This replaces the old external <c>tools/screenshot.ps1</c> helper, so a
    /// screenshot can be taken without leaving the game.
    /// </para>
    /// </summary>
    public static class ScreenshotCapture
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The directory screenshots are written to. This is a <c>Screenshots</c>
        /// folder at the repository root (so in-game captures sit alongside
        /// manually taken ones), falling back to the working directory.
        /// </summary>
        public static string OutputDirectory =>
            Path.Combine(FindRepositoryRoot() ?? Environment.CurrentDirectory, "Screenshots");

        /// <summary>
        /// Reads the back buffer and writes it to a timestamped PNG.
        /// </summary>
        /// <param name="device">The graphics device whose back buffer to capture.</param>
        /// <param name="label">Optional label appended to the file name (e.g. screen name).</param>
        /// <returns>The full path of the written file.</returns>
        public static string Capture(GraphicsDevice device, string label = null)
        {
            ArgumentNullException.ThrowIfNull(device);

            int width = device.PresentationParameters.BackBufferWidth;
            int height = device.PresentationParameters.BackBufferHeight;

            var pixels = new Color[width * height];
            device.GetBackBufferData(pixels);

            using (var texture = new Texture2D(device, width, height))
            {
                texture.SetData(pixels);

                Directory.CreateDirectory(OutputDirectory);

                var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
                var suffix = string.IsNullOrWhiteSpace(label) ? string.Empty : "-" + Sanitize(label);
                var path = Path.Combine(OutputDirectory, stamp + suffix + ".png");

                using (var stream = File.Create(path))
                {
                    texture.SaveAsPng(stream, width, height);
                }

                Logger.Info("Screenshot saved: {0} ({1}x{2})", path, width, height);
                return path;
            }
        }

        private static string Sanitize(string value)
        {
            foreach (var invalid in Path.GetInvalidFileNameChars())
                value = value.Replace(invalid, '_');
            return value;
        }

        /// <summary>
        /// Walks up from the executable's directory looking for the repository
        /// root (marked by <c>AGENTS.md</c> or a <c>.git</c> folder).
        /// </summary>
        private static string FindRepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "AGENTS.md")) ||
                    Directory.Exists(Path.Combine(directory.FullName, ".git")) ||
                    File.Exists(Path.Combine(directory.FullName, ".git")))
                {
                    return directory.FullName;
                }
                directory = directory.Parent;
            }
            return null;
        }
    }
}
