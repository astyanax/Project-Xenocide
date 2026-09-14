using Gum.Forms.Controls;

namespace ProjectXenocide.UI.Controls
{
    /// <summary>
    /// Text style presets matching the Styles.gucx design tokens.
    /// Maps to concrete Text variable values (FontSize/IsBold/IsItalic).
    /// </summary>
    public enum TextStyle
    {
        Tiny,
        Small,
        Normal,
        Emphasis,
        Strong,
        H3,
        H2,
        H1,
        Title,
    }

    /// <summary>
    /// Factory for creating pre-styled Label controls from the Styles.gucx
    /// design token system. Ensures consistent typography across all screens.
    ///
    /// USAGE:
    ///   var title = ThemedLabel.Create("Research", TextStyle.Title);
    ///   var body = ThemedLabel.Create("Select a project", TextStyle.Normal);
    ///   var caption = ThemedLabel.Create("Cost: $500", TextStyle.Small);
    /// </summary>
    public static class ThemedLabel
    {
        /// <summary>
        /// Creates a Label with the specified text and style.
        /// </summary>
        /// <param name="text">Label text.</param>
        /// <param name="style">Text style preset (default: Normal).</param>
        /// <returns>A new Label with the style applied.</returns>
        public static Label Create(string text, TextStyle style = TextStyle.Normal)
        {
            var label = new Label();
            label.Text = text ?? "";
            ApplyStyle(label, style);
            return label;
        }

        /// <summary>Title style: 28px bold.</summary>
        public static Label CreateTitle(string text) => Create(text, TextStyle.Title);

        /// <summary>H3 style: 16px bold.</summary>
        public static Label CreateSection(string text) => Create(text, TextStyle.H3);

        /// <summary>Normal style: 14px.</summary>
        public static Label CreateBody(string text) => Create(text, TextStyle.Normal);

        /// <summary>Small style: 12px.</summary>
        public static Label CreateCaption(string text) => Create(text, TextStyle.Small);

        private static void ApplyStyle(Label label, TextStyle style)
        {
            // Apply the concrete Text variables (FontSize, IsBold, IsItalic) that the
            // Gum text renderer actually honors, matching the values in Styles.gucx.
            // These mirror the child Text instances defined under TextStyles there.
            int fontSize;
            bool isBold;
            bool isItalic;

            switch (style)
            {
                case TextStyle.Tiny:      fontSize = 10; isBold = false; isItalic = false; break;
                case TextStyle.Small:     fontSize = 12; isBold = false; isItalic = false; break;
                case TextStyle.Emphasis:  fontSize = 14; isBold = false; isItalic = true;  break;
                case TextStyle.Strong:    fontSize = 14; isBold = true;  isItalic = false; break;
                case TextStyle.H3:        fontSize = 16; isBold = true;  isItalic = false; break;
                case TextStyle.H2:        fontSize = 18; isBold = true;  isItalic = false; break;
                case TextStyle.H1:        fontSize = 22; isBold = true;  isItalic = false; break;
                case TextStyle.Title:     fontSize = 28; isBold = true;  isItalic = false; break;
                default:                  fontSize = 14; isBold = false; isItalic = false; break;
            }

            label.Visual.SetProperty("FontSize", fontSize);
            label.Visual.SetProperty("IsBold", isBold);
            label.Visual.SetProperty("IsItalic", isItalic);
        }
    }
}
