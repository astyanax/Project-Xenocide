using System.Collections.Generic;

namespace ProjectXenocide.UI
{
    /// <summary>
    /// Central, configurable map from a screen's id to its background texture.
    ///
    /// <para>
    /// Only a handful of full-screen backgrounds exist (start menu, base view,
    /// geoscape, X-Net). Screens without dedicated art fall back to
    /// <see cref="Default"/> — currently the base-view art — so a screen never
    /// silently inherits the geoscape HUD.
    /// </para>
    ///
    /// <para>
    /// To give a screen its own background, add an entry here; no other code
    /// needs to change. Screens may still override via the
    /// <c>Screen(id, backgroundFilename)</c> constructor.
    /// </para>
    /// </summary>
    public static class ScreenBackgrounds
    {
        public const string StartMenu = "Content/Textures/UI/StartScreenBackground.png";
        public const string BaseView = "Content/Textures/UI/BasesScreenBackground.png";
        public const string Geoscape = "Content/Textures/UI/GeoscapeScreenBackground.png";
        public const string XNet = "Content/Textures/UI/XnetScreenBackground.png";

        /// <summary>Background used by any screen without a dedicated entry.</summary>
        public const string Default = BaseView;

        private static readonly Dictionary<string, string> Screens = new()
        {
            ["StartScreen"] = StartMenu,
            ["SettingsScreen"] = StartMenu,
            ["CreditsScreen"] = StartMenu,
            ["GeoscapeScreen"] = Geoscape,
            ["XNetScreen"] = XNet,
            ["BasesScreen"] = BaseView,
            ["BaseInfoScreen"] = BaseView,
            ["SoldiersListScreen"] = BaseView,
            ["AssignToCraftScreen"] = BaseView,
            ["BattlescapeScreen"] = BaseView,
        };

        /// <summary>Background path for the given screen id (falls back to <see cref="Default"/>).</summary>
        public static string For(string screenId)
        {
            if (!string.IsNullOrEmpty(screenId) && Screens.TryGetValue(screenId, out var path))
                return path;
            return Default;
        }
    }
}
