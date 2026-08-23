using System;

using Gum.DataTypes;
using Gum.Forms;
using Gum.Forms.Controls;
using Gum.Wireframe;

using MonoGameGum;

using ProjectXenocide.Assets;

namespace ProjectXenocide.UI.Controls
{
    /// <summary>
    /// Factory for creating themed Buttons, mirroring <see cref="ThemedLabel"/>.
    /// Provides the single source of truth for button creation across screens,
    /// dialogs, and grids so that all buttons share the X-COM 3-slice visual
    /// (the XenocideButton component, textured from XenoNew.png).
    ///
    /// Two visual variants are exposed:
    /// - <see cref="Create"/>: textured XenocideButton (standard action/navigation button).
    /// - <see cref="CreateFlat"/>: plain Forms Button (ButtonStandard component),
    ///   which supports <c>ColorCategoryState</c> — required for the alternating
    ///   row striping used by <see cref="StyledGrid"/>.
    ///
    /// USAGE:
    ///   var ok = ThemedButton.Create("OK", (s, e) => Close());
    ///   panel.AddChild(ok);
    /// </summary>
    public static class ThemedButton
    {
        private static ElementSave buttonTemplate;

        /// <summary>
        /// Creates a textured XenocideButton with the specified text and click handler.
        /// The ButtonClick1 sound is auto-wired unless <paramref name="playSound"/> is false
        /// (set false when the handler plays its own sound, e.g. ButtonClick2).
        /// Falls back to a plain Forms Button if the template is unavailable.
        /// </summary>
        public static Button Create(string text, EventHandler onClick = null, bool playSound = true)
        {
            var button = CreateFromTemplate(text) ?? CreateFallback(text);
            if (playSound)
                button.Click += OnButtonClicked;
            if (onClick != null)
                button.Click += onClick;
            return button;
        }

        /// <summary>
        /// Creates a plain Forms Button (ButtonStandard component) with optional click handler.
        /// Used for grid row buttons and other elements that require ColorCategoryState
        /// styling, which the textured XenocideButton does not support.
        /// </summary>
        public static Button CreateFlat(string text, EventHandler onClick = null)
        {
            var button = new Button { Text = text ?? "" };
            button.Click += OnButtonClicked;
            if (onClick != null)
                button.Click += onClick;
            return button;
        }

        private static Button CreateFromTemplate(string text)
        {
            var project = Xenocide.GumProject;
            if (project == null)
                return null;

            if (buttonTemplate == null)
                buttonTemplate = project.Screens.Find(s => s.Name == "XenocideButtonTemplate");
            if (buttonTemplate == null)
                return null;

            var templateRoot = buttonTemplate.ToGraphicalUiElement();
            var button = templateRoot.GetFrameworkElementByName<Button>("Button");
            if (button == null)
                return null;

            button.Text = text ?? "";
            return button;
        }

        private static Button CreateFallback(string text)
        {
            var button = new Button { Text = text ?? "" };
            button.Visual.Width = 0;
            button.Visual.WidthUnits = DimensionUnitType.RelativeToParent;
            return button;
        }

        private static void OnButtonClicked(object sender, EventArgs e)
        {
            Xenocide.AudioSystem?.PlaySound(SoundId.ButtonClick1);
        }
    }
}
