using System.Collections.Generic;

using Gum.Forms;
using Gum.Forms.Controls;

using MonoGameGum.GueDeriving;

namespace ProjectXenocide.UI.Controls
{
    /// <summary>
    /// Helper for building fixed-size horizontal rows of controls.
    ///
    /// <para>
    /// Gum's <see cref="StackPanel"/> always lays its children out vertically
    /// (its component sets <c>ChildrenLayout = TopToBottom</c>). Callers that want
    /// a left-to-right row — a label beside its control, or a centred row of
    /// action buttons — must therefore position the children explicitly. This
    /// helper provides the row container and the placement maths.
    /// </para>
    /// </summary>
    public static class ThemedRow
    {
        public const int DefaultGap = 8;

        /// <summary>Creates an absolutely sized transparent row container.</summary>
        public static ContainerRuntime Create(float width, float height)
        {
            var row = new ContainerRuntime();
            row.Width = width;
            row.WidthUnits = Gum.DataTypes.DimensionUnitType.Absolute;
            row.Height = height;
            row.HeightUnits = Gum.DataTypes.DimensionUnitType.Absolute;
            return row;
        }

        /// <summary>
        /// Adds a child to <paramref name="row"/> at an absolute position.
        /// </summary>
        public static T Place<T>(ContainerRuntime row, T child, float x, float y = 0)
            where T : FrameworkElement
        {
            if (row == null || child == null)
                return child;

            child.Visual.X = x;
            child.Visual.Y = y;
            child.Visual.XUnits = Gum.Converters.GeneralUnitType.PixelsFromSmall;
            child.Visual.YUnits = Gum.Converters.GeneralUnitType.PixelsFromSmall;
            row.Children.Add(child.Visual);
            return child;
        }

        /// <summary>
        /// Lays <paramref name="buttons"/> out left-to-right, centred horizontally in
        /// the row. Each button's absolute <c>Visual.Width</c> is used.
        /// </summary>
        public static void CenterButtons(ContainerRuntime row, IList<Button> buttons, int gap = DefaultGap)
        {
            if (row == null || buttons == null || buttons.Count == 0)
                return;

            float total = gap * (buttons.Count - 1);
            foreach (var button in buttons)
                total += button.Visual.Width;

            float x = (row.Width - total) / 2f;
            foreach (var button in buttons)
            {
                button.Visual.X = x;
                button.Visual.XUnits = Gum.Converters.GeneralUnitType.PixelsFromSmall;
                x += button.Visual.Width + gap;
            }
        }
    }
}
