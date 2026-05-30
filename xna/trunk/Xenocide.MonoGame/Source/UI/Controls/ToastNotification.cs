using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectXenocide.Utils;

namespace ProjectXenocide.UI.Controls
{
    public class ToastNotification : DrawableGameComponent
    {
        private const float DurationSeconds = 4f;
        private const float FadeSeconds = 0.5f;
        private const int MaxVisible = 4;

        private readonly List<ToastItem> _items = new();
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;

        public ToastNotification(Game game) : base(game)
        {
            DrawOrder = int.MaxValue - 1;
            MessageLog.MessagePosted += OnMessagePosted;
        }

        public override void Initialize()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Game.Content.Load<SpriteFont>("SpriteFont1");
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                _items[i].Elapsed += dt;
                if (_items[i].Elapsed >= DurationSeconds + FadeSeconds)
                    _items.RemoveAt(i);
            }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (_items.Count == 0) return;

            var viewport = GraphicsDevice.Viewport;
            int y = 40;

            _spriteBatch.Begin();

            for (int i = 0; i < _items.Count && i < MaxVisible; i++)
            {
                var item = _items[i];
                float alpha = GetAlpha(item);
                Color bg = GetBackgroundColor(item.Type) * alpha;
                Color textColor = Color.White * alpha;

                string text = Truncate(item.Text, 80);
                var size = _font.MeasureString(text);
                int width = (int)size.X + 20;
                int height = (int)size.Y + 8;
                int x = (viewport.Width - width) / 2;

                var rect = new Rectangle(x, y, width, height);
                _spriteBatch.Draw(GetWhitePixel(), rect, bg);
                _spriteBatch.DrawString(_font, text, new Vector2(x + 10, y + 4), textColor);

                y += height + 4;
            }

            _spriteBatch.End();
        }

        private static float GetAlpha(ToastItem item)
        {
            float remaining = DurationSeconds + FadeSeconds - item.Elapsed;
            if (remaining > DurationSeconds) return 1f;
            return MathHelper.Clamp(remaining / FadeSeconds, 0f, 1f);
        }

        private static Color GetBackgroundColor(MessageType type) => type switch
        {
            MessageType.Warning => new Color(200, 140, 0),
            MessageType.Error => new Color(180, 40, 40),
            MessageType.Required => new Color(30, 80, 180),
            _ => new Color(30, 60, 120),
        };

        private static string Truncate(string text, int maxLen) =>
            text.Length <= maxLen ? text : string.Concat(text.AsSpan(0, maxLen - 3), "...");

        private static Texture2D _whitePixel;
        private Texture2D GetWhitePixel()
        {
            if (_whitePixel == null)
            {
                _whitePixel = new Texture2D(GraphicsDevice, 1, 1);
                _whitePixel.SetData(new[] { Color.White });
            }
            return _whitePixel;
        }

        private void OnMessagePosted(MessageEntry entry)
        {
            _items.Insert(0, new ToastItem { Text = entry.Text, Type = entry.Type, Elapsed = 0 });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                MessageLog.MessagePosted -= OnMessagePosted;
                _spriteBatch?.Dispose();
            }
            base.Dispose(disposing);
        }

        private sealed class ToastItem
        {
            public string Text;
            public MessageType Type;
            public float Elapsed;
        }
    }
}
