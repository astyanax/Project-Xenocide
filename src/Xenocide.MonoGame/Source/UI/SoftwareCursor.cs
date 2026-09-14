using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGameGum;

using ProjectXenocide.UI.Controls;
using ProjectXenocide.UI.Screens;

namespace ProjectXenocide.UI
{
    /// <summary>
    /// Draws the X-COM cursor from <see cref="XenoAtlas"/> and picks the
    /// appropriate frame from context: the standard arrow over UI, and the
    /// targeting reticle while the pointer is over a 3D scene viewport.
    /// </summary>
    public class SoftwareCursor : DrawableGameComponent
    {
        private SpriteBatch _spriteBatch;
        private Texture2D _cursorSheet;

        public static bool IsSoftwareCursorEnabled { get; set; } = true;

        private static readonly Point ArrowHotspot = new Point(0, 0);
        private static readonly Point TargetHotspot = new Point(12, 0);

        private Rectangle _cursorSourceRect = XenoAtlas.Cursors.Arrow;
        private Point _hotspot = ArrowHotspot;

        public SoftwareCursor(Game game) : base(game)
        {
            DrawOrder = int.MaxValue;
        }

        public enum CursorType { Arrow, Target }

        public CursorType CurrentCursorType
        {
            set
            {
                switch (value)
                {
                    case CursorType.Target:
                        _cursorSourceRect = XenoAtlas.Cursors.Target;
                        _hotspot = TargetHotspot;
                        break;
                    default:
                        _cursorSourceRect = XenoAtlas.Cursors.Arrow;
                        _hotspot = ArrowHotspot;
                        break;
                }
            }
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _cursorSheet = XenoAtlas.Texture;

            if (IsSoftwareCursorEnabled)
                Game.IsMouseVisible = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsSoftwareCursorEnabled)
                return;

            CurrentCursorType = DetermineCursorType();
            base.Update(gameTime);
        }

        /// <summary>
        /// Picks the cursor frame from the current pointer context:
        /// targeting reticle over a 3D scene viewport, otherwise the arrow.
        /// </summary>
        private static CursorType DetermineCursorType()
        {
            // Over an interactive Gum Forms control (button, slider, list, ...).
            var overElement = GumService.Default.Cursor?.FrameworkElementOver;
            if (overElement != null)
                return CursorType.Arrow;

            // Over a 3D scene viewport (geoscape globe, battlescape, facility map).
            if (Xenocide.ScreenManager?.TopmostFrame is PolarScreen polar)
            {
                var mouse = Mouse.GetState();
                var vp = Xenocide.Instance.GraphicsDevice.Viewport;
                var rect = polar.ViewportRect;

                int left = (int)(vp.Width * rect.Left);
                int top = (int)(vp.Height * rect.Top);
                int right = (int)(vp.Width * (rect.Left + rect.Width));
                int bottom = (int)(vp.Height * (rect.Top + rect.Height));

                if (mouse.X >= left && mouse.X <= right && mouse.Y >= top && mouse.Y <= bottom)
                    return CursorType.Target;
            }

            return CursorType.Arrow;
        }

        public override void Draw(GameTime gameTime)
        {
            if (!IsSoftwareCursorEnabled || _cursorSheet == null || _spriteBatch == null)
                return;

            var mouse = Mouse.GetState();
            var pos = new Vector2(mouse.X - _hotspot.X, mouse.Y - _hotspot.Y);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp);
            _spriteBatch.Draw(_cursorSheet, pos, _cursorSourceRect, Color.White);
            _spriteBatch.End();
        }
    }
}
