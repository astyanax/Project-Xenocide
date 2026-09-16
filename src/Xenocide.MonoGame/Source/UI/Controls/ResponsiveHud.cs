using Gum.Wireframe;

namespace ProjectXenocide.UI.Controls
{
    /// <summary>
    /// Positions HUD elements that were authored against a fixed design
    /// resolution (1280x1024) so they stay correctly placed at other window
    /// sizes.
    /// </summary>
    /// <remarks>
    /// Elements near the right/bottom edge are anchored to that edge; elements
    /// that nearly span the design width stretch with the window.  At the design
    /// resolution the result matches the authored layout exactly, so this is a
    /// no-op for the default window size.
    /// </remarks>
    public static class ResponsiveHud
    {
        /// <summary>Width of the design resolution the layouts were authored at.</summary>
        public const int DesignWidth = 1280;

        /// <summary>Height of the design resolution the layouts were authored at.</summary>
        public const int DesignHeight = 1024;

        /// <summary>
        /// Repositions an element authored at the given design-space rectangle.
        /// </summary>
        /// <param name="element">Element to position (ignored if null).</param>
        /// <param name="designX">Authored X, in design pixels.</param>
        /// <param name="designY">Authored Y, in design pixels.</param>
        /// <param name="width">Authored width, in design pixels.</param>
        /// <param name="height">Authored height, in design pixels; 0 leaves it unchanged.</param>
        public static void Position(GraphicalUiElement element, int designX, int designY, int width, int height = 0)
        {
            if ((element == null) || (Xenocide.Instance?.GraphicsDevice == null))
                return;

            int rightMargin = DesignWidth - (designX + width);

            if ((designX <= 40) && (rightMargin <= 40))
            {
                // nearly full width: stretch between the design margins
                element.XOrigin = RenderingLibrary.Graphics.HorizontalAlignment.Left;
                element.XUnits = Gum.Converters.GeneralUnitType.PixelsFromSmall;
                element.X = designX;
                element.WidthUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;
                element.Width = -(designX + rightMargin);
            }
            else if (rightMargin <= 200)
            {
                // near the right edge: pin it there
                element.XOrigin = RenderingLibrary.Graphics.HorizontalAlignment.Right;
                element.XUnits = Gum.Converters.GeneralUnitType.PixelsFromLarge;
                element.X = -rightMargin;
                element.WidthUnits = Gum.DataTypes.DimensionUnitType.Absolute;
                element.Width = width;
            }
            else
            {
                element.XOrigin = RenderingLibrary.Graphics.HorizontalAlignment.Left;
                element.XUnits = Gum.Converters.GeneralUnitType.PixelsFromSmall;
                element.X = designX;
                element.WidthUnits = Gum.DataTypes.DimensionUnitType.Absolute;
                element.Width = width;
            }

            if (height > 0)
            {
                element.HeightUnits = Gum.DataTypes.DimensionUnitType.Absolute;
                element.Height = height;
            }

            // Vertical: hug the bottom edge when within an obvious margin.
            int bottomMargin = DesignHeight - (designY + height);
            if ((height > 0) && (bottomMargin <= 320))
            {
                element.YOrigin = RenderingLibrary.Graphics.VerticalAlignment.Bottom;
                element.YUnits = Gum.Converters.GeneralUnitType.PixelsFromLarge;
                element.Y = -bottomMargin;
            }
            else
            {
                element.YOrigin = RenderingLibrary.Graphics.VerticalAlignment.Top;
                element.YUnits = Gum.Converters.GeneralUnitType.PixelsFromSmall;
                element.Y = designY;
            }
        }
    }
}
