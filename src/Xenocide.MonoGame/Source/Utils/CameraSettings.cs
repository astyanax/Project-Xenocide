using System;

using ProjectXenocide.Model;

namespace ProjectXenocide.Utils
{
    /// <summary>User preferences for the 3D globe camera. Persisted via GameOptions.</summary>
    public static class CameraSettings
    {
        /// <summary>Rotation speed multiplier (drag).</summary>
        public static float RotateSensitivity { get; set; } = 0.6f;

        /// <summary>Zoom speed multiplier (wheel/buttons).</summary>
        public static float ZoomSensitivity { get; set; } = 1.0f;

        /// <summary>Invert vertical drag (grab-the-globe feel).</summary>
        public static bool InvertY { get; set; } = true;

        /// <summary>Zoom toward the mouse cursor rather than the globe centre.</summary>
        public static bool ZoomToCursor { get; set; } = true;

        public static void Load(GameOptions options)
        {
            if (options == null)
            {
                return;
            }
            RotateSensitivity = Math.Clamp(options.CameraRotateSensitivity, 0.1f, 3.0f);
            ZoomSensitivity = Math.Clamp(options.CameraZoomSensitivity, 0.1f, 3.0f);
            InvertY = options.CameraInvertY;
            ZoomToCursor = options.CameraZoomToCursor;
        }

        public static void Save(GameOptions options)
        {
            if (options == null)
            {
                return;
            }
            options.CameraRotateSensitivity = RotateSensitivity;
            options.CameraZoomSensitivity = ZoomSensitivity;
            options.CameraInvertY = InvertY;
            options.CameraZoomToCursor = ZoomToCursor;
        }
    }
}
