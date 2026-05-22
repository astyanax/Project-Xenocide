using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ProjectXenocide.UI
{
    public class SoftwareCursor : DrawableGameComponent
    {
        private SpriteBatch _spriteBatch;
        private Texture2D _cursorSheet;
        private Rectangle _cursorSourceRect;

        public SoftwareCursor(Game game) : base(game)
        {
            DrawOrder = int.MaxValue;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _cursorSheet = Texture2D.FromFile(GraphicsDevice, "Content/Textures/UI/XenoNew.png");
            _cursorSourceRect = new Rectangle(142, 127, 24, 24);

            Game.IsMouseVisible = false;
        }

        public override void Draw(GameTime gameTime)
        {
            var mouse = Mouse.GetState();
            var pos = new Vector2(mouse.X, mouse.Y);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp);
            _spriteBatch.Draw(_cursorSheet, pos, _cursorSourceRect, Color.White);
            _spriteBatch.End();
        }
    }
}
