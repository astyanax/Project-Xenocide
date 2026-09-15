using System;

using Gum.Converters;
using Gum.DataTypes;

using Microsoft.Xna.Framework;

using MonoGameGum.GueDeriving;

using RenderingLibrary.Graphics;

namespace ProjectXenocide.UI.Controls
{
    /// <summary>
    /// A 9-slice frame assembled from nine <see cref="SpriteRuntime"/>s cut from a
    /// single atlas region.
    ///
    /// <para>
    /// This exists because Gum's <c>NineSliceRuntime</c> only supports a
    /// <em>symmetric</em> border (a single <c>CustomFrameTextureCoordinateWidth</c>
    /// applied to both axes). The X-COM window frame has an asymmetric border — a
    /// tall title bar on top, thinner sides/bottom — so it cannot be expressed as a
    /// symmetric nine-slice.
    /// </para>
    ///
    /// <para>
    /// The four corners are drawn at their native pixel size; the four edges stretch
    /// along their length only; the centre stretches both ways. Nothing is ever
    /// scaled across a border, so there is no distortion. All pieces use relative
    /// units, so the frame re-lays-out automatically whenever the host element is
    /// resized (e.g. at a different resolution).
    /// </para>
    /// </summary>
    public class AtlasNineSlice
    {
        /// <summary>Root element; set its size and add it to the panel.</summary>
        public ContainerRuntime Visual { get; }

        public AtlasNineSlice(Rectangle source, int left, int right, int top, int bottom)
        {
            Visual = new ContainerRuntime();
            Visual.Width = 0;
            Visual.WidthUnits = DimensionUnitType.RelativeToParent;
            Visual.Height = 0;
            Visual.HeightUnits = DimensionUnitType.RelativeToParent;

            int midW = source.Width - left - right;
            int midH = source.Height - top - bottom;

            Rectangle tl = new Rectangle(source.X, source.Y, left, top);
            Rectangle tr = new Rectangle(source.X + source.Width - right, source.Y, right, top);
            Rectangle tt = new Rectangle(source.X + left, source.Y, midW, top);
            Rectangle ll = new Rectangle(source.X, source.Y + top, left, midH);
            Rectangle cc = new Rectangle(source.X + left, source.Y + top, midW, midH);
            Rectangle rr = new Rectangle(source.X + source.Width - right, source.Y + top, right, midH);
            Rectangle bl = new Rectangle(source.X, source.Y + source.Height - bottom, left, bottom);
            Rectangle bb = new Rectangle(source.X + left, source.Y + source.Height - bottom, midW, bottom);
            Rectangle br = new Rectangle(source.X + source.Width - right, source.Y + source.Height - bottom, right, bottom);

            // Corners — fixed size, anchored to their respective corner.
            Add(tl, s => Place(s, HorizontalAlignment.Left, VerticalAlignment.Top, 0, 0, left, top));
            Add(tr, s => Place(s, HorizontalAlignment.Right, VerticalAlignment.Top, 0, 0, right, top));
            Add(bl, s => Place(s, HorizontalAlignment.Left, VerticalAlignment.Bottom, 0, 0, left, bottom));
            Add(br, s => Place(s, HorizontalAlignment.Right, VerticalAlignment.Bottom, 0, 0, right, bottom));

            // Edges — stretch along their length, fixed across it.
            Add(tt, s => Place(s, HorizontalAlignment.Left, VerticalAlignment.Top, left, 0, -(left + right), top, stretchWidth: true));
            Add(bb, s => Place(s, HorizontalAlignment.Left, VerticalAlignment.Bottom, left, 0, -(left + right), bottom, stretchWidth: true));
            Add(ll, s => Place(s, HorizontalAlignment.Left, VerticalAlignment.Top, 0, top, left, -(top + bottom), stretchHeight: true));
            Add(rr, s => Place(s, HorizontalAlignment.Right, VerticalAlignment.Top, 0, top, right, -(top + bottom), stretchHeight: true));

            // Centre — stretches both ways.
            Add(cc, s => Place(s, HorizontalAlignment.Left, VerticalAlignment.Top, left, top, -(left + right), -(top + bottom), stretchWidth: true, stretchHeight: true));
        }

        private void Add(Rectangle source, Action<SpriteRuntime> configure)
        {
            var sprite = XenoAtlas.CreateSprite(source);
            if (sprite == null)
                return;

            configure(sprite);
            Visual.Children.Add(sprite);
        }

        private static void Place(
            SpriteRuntime sprite,
            HorizontalAlignment xOrigin,
            VerticalAlignment yOrigin,
            float x,
            float y,
            float width,
            float height,
            bool stretchWidth = false,
            bool stretchHeight = false)
        {
            sprite.XOrigin = xOrigin;
            sprite.XUnits = xOrigin == HorizontalAlignment.Right ? GeneralUnitType.PixelsFromLarge : GeneralUnitType.PixelsFromSmall;
            sprite.X = x;

            sprite.YOrigin = yOrigin;
            sprite.YUnits = yOrigin == VerticalAlignment.Bottom ? GeneralUnitType.PixelsFromLarge : GeneralUnitType.PixelsFromSmall;
            sprite.Y = y;

            sprite.WidthUnits = stretchWidth ? DimensionUnitType.RelativeToParent : DimensionUnitType.Absolute;
            sprite.Width = width;

            sprite.HeightUnits = stretchHeight ? DimensionUnitType.RelativeToParent : DimensionUnitType.Absolute;
            sprite.Height = height;
        }
    }
}
