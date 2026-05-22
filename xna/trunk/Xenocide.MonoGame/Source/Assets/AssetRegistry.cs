using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectXenocide.Assets
{
    public static class AssetRegistry
    {
        public static readonly Dictionary<SoundId, string> SoundPaths = new()
        {
            [SoundId.ButtonClick1] = "Menu/buttonclick1_ok.ogg",
            [SoundId.ButtonClick2] = "Menu/buttonclick2_changesetting.ogg",
            [SoundId.ButtonOver] = "Menu/buttonover.ogg",
            [SoundId.ExitGame] = "Menu/exitgame.ogg",
            [SoundId.PlanetViewSpeedFast] = "PlanetView/speedfast.ogg",
            [SoundId.PlanetViewSpeedSlow] = "PlanetView/speedslow.ogg",
            [SoundId.PlanetViewSpeedVeryFast] = "PlanetView/speedveryfast.ogg",
            [SoundId.PlanetViewZoomIn] = "PlanetView/zoomin.ogg",
            [SoundId.PlanetViewZoomOut] = "PlanetView/zoomout.ogg",
            [SoundId.PlanetViewClickObject] = "PlanetView/clickobjectonplanet.ogg",
        };

        public static readonly Dictionary<MusicId, (string AssetName, string Category)> MusicDefs = new()
        {
            [MusicId.MainTheme] = ("Audio/Music/main_theme", "MainMenu"),
            [MusicId.Planetview] = ("Audio/Music/Planetview/planetview", "PlanetView"),
            [MusicId.PlanetviewTiskaite] = ("Audio/Music/Planetview/Tiskaite_-_Xenocide_Geoscape", "PlanetView"),
            [MusicId.PlanetviewThomas] = ("Audio/Music/Planetview/10. Thomas Torfs - Planetview", "PlanetView"),
            [MusicId.Baseview] = ("Audio/Music/Baseview/7. XerO - Baseview", "BaseView"),
            [MusicId.XNet] = ("Audio/Music/XNet/xnet", "XNet"),
        };

        public static readonly Dictionary<TextureId, string> TextureContentPaths = new()
        {
            [TextureId.IconSpriteMap] = "Textures/Geoscape/IconSpriteMap",
            [TextureId.GeoscapeSkybox] = "Textures/Geoscape/skybox",
            [TextureId.BuildTimes] = "Textures/OutpostLayout/BuildTimes",
        };

        public static readonly Dictionary<TextureId, string> TextureFilePaths = new()
        {
            [TextureId.SkyboxPng] = "Content/Textures/Geoscape/skybox.png",
            [TextureId.EarthDiffuseMap] = "Content/Textures/Geoscape/EarthDiffuseMap.jpg",
            [TextureId.EarthNightMap] = "Content/Textures/Geoscape/EarthNightMap.png",
            [TextureId.EarthNormalMap] = "Content/Textures/Geoscape/EarthNormalMap.png",
            [TextureId.EquipScreenBackground] = "Content/Textures/EquipSoldier/EquipScreenBackground.png",
            [TextureId.InventorySprites] = "Content/Textures/EquipSoldier/InventorySprites.png",
            [TextureId.BattlescapeTextureAtlas] = "Content/Textures/Battlescape/textureAtlas.png",
        };

        public static readonly Dictionary<UIBackgroundId, string> UIBackgroundPaths = new()
        {
            [UIBackgroundId.StartScreen] = "Content/Textures/UI/StartScreenBackground.png",
            [UIBackgroundId.BasesScreen] = "Content/Textures/UI/BasesScreenBackground.png",
            [UIBackgroundId.XnetScreen] = "Content/Textures/UI/XnetScreenBackground.png",
            [UIBackgroundId.GeoscapeScreen] = "Content/Textures/UI/GeoscapeScreenBackground.png",
        };

        public static readonly Dictionary<ShaderId, string> ShaderPaths = new()
        {
            [ShaderId.GeoscapeShader] = "Shaders/GeoscapeShader",
            [ShaderId.Skybox] = "Shaders/skybox",
        };

        public static readonly Dictionary<FontId, string> FontPaths = new()
        {
            [FontId.SpriteFont1] = "SpriteFont1",
            [FontId.Xeno] = "SpriteFonts/Xeno",
            [FontId.XenoBig] = "SpriteFonts/XenoBig",
            [FontId.LargeBaseName] = "SpriteFonts/LargeBaseName",
            [FontId.GeoTime] = "SpriteFonts/GeoTime",
            [FontId.GeoTimeBig] = "SpriteFonts/GeoTimeBig",
        };

        public static string SoundPath(SoundId id) => SoundPaths[id];
        public static (string assetName, string category) MusicDef(MusicId id) => MusicDefs[id];
        public static string TextureContentPath(TextureId id) => TextureContentPaths[id];
        public static string TextureFilePath(TextureId id) => TextureFilePaths[id];
        public static string UIBackgroundPath(UIBackgroundId id) => UIBackgroundPaths[id];
        public static string ShaderPath(ShaderId id) => ShaderPaths[id];
        public static string FontPath(FontId id) => FontPaths[id];
    }
}
