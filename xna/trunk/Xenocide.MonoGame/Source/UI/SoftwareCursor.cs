using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ProjectXenocide.UI
{
    public class SoftwareCursor : DrawableGameComponent
    {
        private SpriteBatch _spriteBatch;
        private Texture2D _cursorSheet;

        public static bool IsSoftwareCursorEnabled { get; set; } = true;

        private static readonly Rectangle DefaultCursorRect = new Rectangle(142, 127, 24, 24);
        private static readonly Rectangle PointerCursorRect = new Rectangle(166, 127, 24, 24);
        private static readonly Rectangle HandCursorRect = new Rectangle(190, 127, 24, 24);
        private static readonly Point DefaultHotspot = new Point(0, 0);
        private static readonly Point PointerHotspot = new Point(12, 0);
        private static readonly Point HandHotspot = new Point(8, 4);

        private Rectangle _cursorSourceRect = DefaultCursorRect;
        private Point _hotspot = DefaultHotspot;

        public SoftwareCursor(Game game) : base(game)
        {
            DrawOrder = int.MaxValue;
        }

        public enum CursorType { Default, Pointer, Hand }

        public CursorType CurrentCursorType
        {
            set
            {
                switch (value)
                {
                    case CursorType.Pointer:
                        _cursorSourceRect = PointerCursorRect;
                        _hotspot = PointerHotspot;
                        break;
                    case CursorType.Hand:
                        _cursorSourceRect = HandCursorRect;
                        _hotspot = HandHotspot;
                        break;
                    default:
                        _cursorSourceRect = DefaultCursorRect;
                        _hotspot = DefaultHotspot;
                        break;
                }
            }
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _cursorSheet = Texture2D.FromFile(GraphicsDevice, "Content/Textures/UI/XenoNew.png");

            if (IsSoftwareCursorEnabled)
                Game.IsMouseVisible = false;
        }

        public override void Draw(GameTime gameTime)
        {
            if (!IsSoftwareCursorEnabled) return;

            var mouse = Mouse.GetState();
            var pos = new Vector2(mouse.X - _hotspot.X, mouse.Y - _hotspot.Y);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp);
            _spriteBatch.Draw(_cursorSheet, pos, _cursorSourceRect, Color.White);
            _spriteBatch.End();
        }
    }
}
