using ProjectXenocide.Model;

namespace ProjectXenocide.Utils
{
    /// <summary>Which sky projection to use on the geoscape. Persisted via GameOptions.</summary>
    public static class SkyboxSettings
    {
        /// <summary>Auto (default) lets the factory pick; currently equirectangular.</summary>
        public static SkyboxMode Mode { get; set; } = SkyboxMode.Auto;

        public static void Load(GameOptions options)
        {
            if (options == null)
            {
                return;
            }
            Mode = options.SkyboxMode;
        }

        public static void Save(GameOptions options)
        {
            if (options == null)
            {
                return;
            }
            options.SkyboxMode = Mode;
        }
    }
}
