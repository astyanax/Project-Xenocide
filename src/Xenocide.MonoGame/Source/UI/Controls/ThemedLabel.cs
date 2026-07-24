using Gum.Forms.Controls;

namespace ProjectXenocide.UI.Controls
{
    /// <summary>
    /// Text style presets matching the Styles.gucx design tokens.
    /// Maps to Gum's StyleCategoryState values.
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

        /// <summary>H1 style: 22px bold.</summary>
        public static Label CreateHeader(string text) => Create(text, TextStyle.H1);

        /// <summary>H2 style: 18px bold.</summary>
        public static Label CreateSubHeader(string text) => Create(text, TextStyle.H2);

        /// <summary>H3 style: 16px bold.</summary>
        public static Label CreateSection(string text) => Create(text, TextStyle.H3);

        /// <summary>Normal style: 14px.</summary>
        public static Label CreateBody(string text) => Create(text, TextStyle.Normal);

        /// <summary>Small style: 12px.</summary>
        public static Label CreateCaption(string text) => Create(text, TextStyle.Small);

        /// <summary>Tiny style: 10px.</summary>
        public static Label CreateMicro(string text) => Create(text, TextStyle.Tiny);

        /// <summary>Strong style: 14px bold.</summary>
        public static Label CreateStrong(string text) => Create(text, TextStyle.Strong);

        /// <summary>Emphasis style: 14px italic.</summary>
        public static Label CreateEmphasis(string text) => Create(text, TextStyle.Emphasis);

        private static void ApplyStyle(Label label, TextStyle style)
        {
            string styleName = style switch
            {
                TextStyle.Tiny => "Tiny",
                TextStyle.Small => "Small",
                TextStyle.Normal => "Normal",
                TextStyle.Emphasis => "Emphasis",
                TextStyle.Strong => "Strong",
                TextStyle.H3 => "H3",
                TextStyle.H2 => "H2",
                TextStyle.H1 => "H1",
                TextStyle.Title => "Title",
                _ => "Normal",
            };
            label.Visual.SetProperty("StyleCategoryState", styleName);
        }
    }
}
