/*
 * SpriteItem and SpriteMap classes are from the Ziggyware tutorials section.
 * 
 * http://www.ziggyware.com/readarticle.php?article_id=141
 * 
 * /*
* @file SpriteMap.cs
* @date Created: 2009/10/21
* @author File creator: John Perrin
* @author Credits: the Ziggyware tutorials section
*/


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectXenocide.Utils
{
    /// <summary>
    /// This structure just stores a texture and rectangle specifying the area within the
    /// texture which will be rendered.
    /// </summary>
    public struct SpriteItem
    {
        private Texture2D tex;
        private Rectangle srcrect;

        public SpriteItem(Texture2D texture, Rectangle srcrect)
        {
            this.tex = texture;
            this.srcrect = srcrect;
        }

        public int Width
        {
            get { return srcrect.Width; }
        }

        public int Height
        {
            get { return srcrect.Height; }
        }

        public void Draw(SpriteBatch batch, Rectangle destrect, Color color)
        {
            batch.Draw(tex, destrect, srcrect, color);
        }

        public void Draw(SpriteBatch batch, Vector2 pos, Color color)
        {
            batch.Draw(tex, pos, srcrect, color);
        }

        public void Draw(SpriteBatch batch, Rectangle destrect, Color color, float rotation, Vector2 origin, SpriteEffects effect, float layerdepth)
        {
            batch.Draw(tex, destrect, srcrect, color, rotation, origin, effect, layerdepth);
        }

        public void Draw(SpriteBatch batch, Vector2 pos, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effect, float layerdepth)
        {
            batch.Draw(tex, pos, srcrect, color, rotation, origin, scale, effect, layerdepth);
        }

        public void Draw(SpriteBatch batch, Vector2 pos, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float layerdepth)
        {
            batch.Draw(tex, pos, srcrect, color, rotation, origin, scale, effect, layerdepth);
        }
    }

    public class SpriteMap
    {
        private Texture2D tex;
        private int gridx, gridy;
        private int mulx, muly;

        public SpriteMap(Texture2D texture, int gridx, int gridy)
        {
            this.tex = texture;
            this.gridx = gridx;
            this.gridy = gridy;

            // Calculate the width and height of each tile within the sprite map.
            this.mulx = tex.Width / gridx;
            this.muly = tex.Height / gridy;
        }

        public SpriteItem this[int frame]
        {
            get
            {
                // Extract the right area of the sprite for the given frame.
                int x = frame % gridx;
                int y = frame / gridx;
                Rectangle srcrect = new Rectangle(x * mulx, y * muly, mulx, muly);

                // Create a SpriteItem object and return it to the caller.
                return new SpriteItem(tex, srcrect);
            }
        }

        public void Dispose()
        {
            tex.Dispose();
        }
    }
}
