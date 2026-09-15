using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectXenocide.Model
{
    /// <summary>How the game window is presented.</summary>
    public enum WindowMode
    {
        /// <summary>Normal resizable window.</summary>
        Windowed = 0,

        /// <summary>
        /// Full-screen but without an exclusive display-mode switch. Toggling is
        /// instant (no black-screen/freeze) and Alt-Tab works normally.
        /// </summary>
        Borderless = 1,

        /// <summary>Classic exclusive full-screen (changes the display mode).</summary>
        Exclusive = 2,
    }

    /// <summary>Applies a <see cref="WindowMode"/> to a graphics device manager.</summary>
    public static class DisplayModeManager
    {
        public static void Apply(GraphicsDeviceManager graphics, WindowMode mode)
        {
            if (graphics == null)
                return;

            switch (mode)
            {
                case WindowMode.Windowed:
                    graphics.HardwareModeSwitch = false;
                    graphics.IsFullScreen = false;
                    break;

                case WindowMode.Exclusive:
                    graphics.HardwareModeSwitch = true;
                    graphics.IsFullScreen = true;
                    break;

                case WindowMode.Borderless:
                default:
                    // No hardware mode switch => borderless full-screen.
                    graphics.HardwareModeSwitch = false;
                    graphics.IsFullScreen = true;
                    break;
            }

            graphics.ApplyChanges();
        }

        /// <summary>Human-readable label for the settings screen.</summary>
        public static string Label(WindowMode mode)
        {
            switch (mode)
            {
                case WindowMode.Windowed: return "Windowed";
                case WindowMode.Exclusive: return "Fullscreen (exclusive)";
                default: return "Fullscreen (borderless)";
            }
        }
    }
}
