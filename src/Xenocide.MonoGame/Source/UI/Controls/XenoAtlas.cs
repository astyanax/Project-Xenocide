using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectXenocide.UI.Controls
{
    /// <summary>
    /// Named regions of <c>XenoNew.png</c>, the X-COM UI sprite atlas (512×512).
    /// Central place for texture coordinates so screens/dialogs/cursors reference
    /// sprites by name instead of magic pixel numbers (as the .gucx files and
    /// <see cref="SoftwareCursor"/> historically did).
    ///
    /// The atlas is loaded once, lazily, via <see cref="Texture"/>.
    /// Coordinates were derived from visual inspection / connected-component
    /// analysis of the atlas and are approximate; refine them as the art is
    /// formalised.
    /// </summary>
    public static class XenoAtlas
    {
        /// <summary>Path relative to the game working directory.</summary>
        public const string TexturePath = "Content/Textures/UI/XenoNew.png";

        /// <summary>Atlas dimensions in pixels.</summary>
        public const int Width = 512;
        public const int Height = 512;

        private static Texture2D texture;

        /// <summary>
        /// The shared atlas texture. Returns null until a GraphicsDevice is
        /// available (e.g. very early in startup, or in headless tests).
        /// </summary>
        public static Texture2D Texture
        {
            get
            {
                if (texture == null && Xenocide.Instance?.GraphicsDevice != null)
                    texture = Texture2D.FromFile(Xenocide.Instance.GraphicsDevice, TexturePath);
                return texture;
            }
        }

        /// <summary>Creates a SpriteRuntime showing the given atlas region, or null if unavailable.</summary>
        public static MonoGameGum.GueDeriving.SpriteRuntime CreateSprite(Rectangle source)
        {
            var atlas = Texture;
            if (atlas == null)
                return null;

            var sprite = new MonoGameGum.GueDeriving.SpriteRuntime
            {
                Texture = atlas,
                SourceRectangle = source
            };
            return sprite;
        }

        /// <summary>Large background/frame panels.</summary>
        public static class Panels
        {
            /// <summary>Slate scanline panel — dialog/content background fill.</summary>
            public static readonly Rectangle ContentBackground = new(0, 237, 153, 138);

            /// <summary>Full window chrome incl. title bar and close button.</summary>
            public static readonly Rectangle WindowFrame = new(251, 229, 217, 236);

            /// <summary>Title-bar strip of the window chrome.</summary>
            public static readonly Rectangle TitleBar = new(253, 231, 213, 28);

            /// <summary>Close ("X") button at the window chrome's top-right.</summary>
            public static readonly Rectangle CloseButton = new(486, 234, 18, 18);
        }

        /// <summary>Bar / fill states (greens).</summary>
        public static class Bars
        {
            /// <summary>Solid dark-green fill (e.g. progress/button state).</summary>
            public static readonly Rectangle GreenSolid = new(148, 389, 31, 17);

            /// <summary>Solid dark-green fill (second row).</summary>
            public static readonly Rectangle GreenSolidAlt = new(148, 420, 32, 17);

            /// <summary>Hatched green fill (striped highlight).</summary>
            public static readonly Rectangle GreenStriped = new(192, 388, 50, 51);
        }

        /// <summary>Button 3-slice rows consumed by XenocideButton.gucx.</summary>
        public static class Buttons
        {
            public static readonly Rectangle Enabled = new(266, 131, 178, 23);
            public static readonly Rectangle Highlighted = new(262, 164, 182, 23);
            public static readonly Rectangle Pushed = new(262, 196, 182, 23);
        }

        /// <summary>Mouse cursors (24×24 each).</summary>
        public static class Cursors
        {
            public static readonly Rectangle Default = new(142, 127, 24, 24);
            public static readonly Rectangle Arrow = new(166, 127, 24, 24);
            public static readonly Rectangle Hand = new(190, 127, 24, 24);
        }

        /// <summary>Four-point compass/waypoint stars.</summary>
        public static class Markers
        {
            public static readonly Rectangle CompassLight = new(9, 382, 54, 54);
            public static readonly Rectangle CompassDark = new(7, 441, 56, 52);
        }

        /// <summary>Zoom / scroll controls.</summary>
        public static class Controls
        {
            public static readonly Rectangle ZoomIn = new(109, 391, 20, 22);
            public static readonly Rectangle ZoomOut = new(109, 450, 20, 22);
            public static readonly Rectangle Minus = new(81, 420, 17, 4);
            public static readonly Rectangle SliderThumb = new(156, 231, 18, 34);
        }

        /// <summary>Status/tech icons (greens).</summary>
        public static class Icons
        {
            public static readonly Rectangle CloseX = new(186, 261, 22, 21);
            public static readonly Rectangle Wifi = new(186, 353, 23, 24);
        }
    }
}
