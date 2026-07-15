using System.Collections.Generic;

namespace ProjectXenocide.Assets
{
    /// <summary>
    /// Font identifiers for sprite fonts registered in the MonoGame Content Pipeline.
    ///
    /// These correspond to .spritefont files built by MGCB and registered in
    /// AssetRegistry.FontPaths. Each entry maps to a ContentManager asset path.
    ///
    /// The legacy CeGui font system (removed) used FontManager.CreateFont() with
    /// system-installed fonts at specific sizes. During the Gum migration, those
    /// references were converted to sprite fonts. The LegacyCeGuiFont table below
    /// documents every font the old system created so we know what each was used for
    /// when integrating proper custom fonts later.
    /// </summary>
    public enum FontId
    {
        /// <summary>
        /// General-purpose default font. Replaces the old CeGui "Default" font
        /// (Arial, 8pt). Used anywhere a screen doesn't specify a particular font.
        /// </summary>
        SpriteFont1,

        /// <summary>
        /// Button and label text. Replaces the old CeGui "Xeno" font (Gotthard, 8pt).
        /// XenocideButton component defaults to this via Gum's TextInstance.
        /// </summary>
        Xeno,

        /// <summary>
        /// Large emphasis text (funds display, etc.). Replaces "XenoBig" (Gotthard, 10pt Bold).
        /// </summary>
        XenoBig,

        /// <summary>
        /// Small button text (time-speed buttons, camera d-pad). Replaces "XenoSmall" (Gotthard, 6pt).
        /// Used in GeoscapeScreen for timeNormalButton, cameraUpButton, etc.
        /// </summary>
        XenoSmall,

        /// <summary>
        /// Outpost/base name display. Replaces "LargeBaseName" (Arial, 24pt Bold).
        /// Used in BasesScreen for the large base name label.
        /// </summary>
        LargeBaseName,

        /// <summary>
        /// Geoscape time display. Replaces "GeoTime" (OCR A Extended, 10pt Bold).
        /// Used in GeoscapeScreen for gameTimeTop, gameTimeSec, timeText.
        /// </summary>
        GeoTime,

        /// <summary>
        /// Geoscape large time display. Replaces "GeoTimeBig" (OCR A Extended, 18pt Bold).
        /// Used in GeoscapeScreen for gameTimeHour.
        /// </summary>
        GeoTimeBig,

        /// <summary>
        /// Generic monospace fallback. Not from the legacy CeGui system; added during
        /// the Gum migration as a catch-all for any unmapped legacy font name.
        /// </summary>
        Arial,
    }

    /// <summary>
    /// Central font registry: maps legacy CeGui font names and FontId enum values
    /// to the modern Gum font family / sprite font path.
    ///
    /// The LegacyCeGuiFont table documents every font created in the old
    /// InitializeCeGui() method. Each entry records:
    ///   - CeGuiName: the first argument to FontManager.CreateFont()
    ///   - SystemFont: the system-installed font face that was used
    ///   - Size: point size
    ///   - Style: Bold or None
    ///   - Usage: the game features/screens that used it
    ///   - Replacement: the FontId that replaces it in the modern system
    ///
    /// The FontNameMap dictionary is used at runtime (in GeoscapeScreen.SetFont)
    /// to resolve legacy string font names to FontId values, defaulting to Arial
    /// for any unrecognized name.
    /// </summary>
    public static class FontRegistry
    {
        /// <summary>
        /// Complete record of every font registered by the legacy CeGui InitializeCeGui().
        /// Preserved so we know exactly where each font was used when integrating
        /// proper custom fonts (e.g. OCR A Extended for geo-time) later.
        /// </summary>
        public static readonly IReadOnlyList<LegacyCeGuiFont> LegacyCeGuiFonts = new List<LegacyCeGuiFont>
        {
            new("Default",     "Arial",           8,  LegacyFontStyle.None,  "System default for all UI elements",                          FontId.SpriteFont1),
            new("Xeno",        "Gotthard",        8,  LegacyFontStyle.None,  "Standard button labels across all screens",                  FontId.Xeno),
            new("XenoBig",     "Gotthard",        10, LegacyFontStyle.Bold,  "Funds display, large emphasis text",                         FontId.XenoBig),
            new("XenoSmall",   "Gotthard",        6,  LegacyFontStyle.None,  "Small buttons: Geoscape time-speed and camera d-pad",       FontId.XenoSmall),
            new("LargeBaseName","Arial",          24, LegacyFontStyle.Bold,  "Outpost/base name in BasesScreen",                          FontId.LargeBaseName),
            new("GeoTime",     "OCR A Extended",  10, LegacyFontStyle.Bold,  "Geoscape time display (gameTimeTop, gameTimeSec, timeText)", FontId.GeoTime),
            new("GeoTimeBig",  "OCR A Extended",  18, LegacyFontStyle.Bold,  "Geoscape hour display (gameTimeHour)",                      FontId.GeoTimeBig),
        };

        /// <summary>
        /// Maps legacy CeGui font name strings (e.g. "XenoSmall", "GeoTime") to
        /// their modern FontId replacements. Used at runtime by GeoscapeScreen.SetFont
        /// to translate old-style string font references.
        /// Any name not in this map falls back to FontId.Arial.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, FontId> FontNameMap = new Dictionary<string, FontId>
        {
            ["Default"]         = FontId.SpriteFont1,
            ["Xeno"]            = FontId.Xeno,
            ["XenoBig"]         = FontId.XenoBig,
            ["XenoSmall"]       = FontId.XenoSmall,
            ["LargeBaseName"]   = FontId.LargeBaseName,
            ["GeoTime"]         = FontId.GeoTime,
            ["GeoTimeBig"]      = FontId.GeoTimeBig,
        };

        /// <summary>
        /// Returns the font family string for a given FontId.
        /// Used by Gum's TextRuntime.SetProperty("Font", family) to apply the font.
        /// Falls back to "Arial" for unrecognized ids.
        /// </summary>
        public static string GetFontFamily(FontId id) => id switch
        {
            FontId.SpriteFont1  => "Arial",
            FontId.Xeno         => "Gotthard",
            FontId.XenoBig      => "Gotthard",
            FontId.XenoSmall    => "Gotthard",
            FontId.LargeBaseName => "Arial",
            FontId.GeoTime      => "Arial",
            FontId.GeoTimeBig   => "Arial",
            FontId.Arial        => "Arial",
            _                   => "Arial",
        };
    }

    /// <summary>
    /// Describes one font registered in the legacy CeGui InitializeCeGui() method.
    /// Each entry records what the old system used so we can integrate equivalent
    /// fonts in Gum.
    /// </summary>
    /// <param name="CeGuiName">First arg to FontManager.CreateFont — the identifier used in CeGui code.</param>
    /// <param name="SystemFont">System-installed font face (e.g. "Gotthard", "OCR A Extended").</param>
    /// <param name="Size">Point size.</param>
    /// <param name="Style">Bold or None.</param>
    /// <param name="Usage">What game feature/screen used this font.</param>
    /// <param name="Replacement">The FontId that replaces it in the Gum-based UI.</param>
    public record LegacyCeGuiFont(
        string CeGuiName,
        string SystemFont,
        int Size,
        LegacyFontStyle Style,
        string Usage,
        FontId Replacement
    );

    /// <summary>
    /// Font style flags used by the legacy CeGui FontManager.
    /// </summary>
    public enum LegacyFontStyle
    {
        None = 0,
        Bold = 1,
    }
}
