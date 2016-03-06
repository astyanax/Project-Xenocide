using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Drawing;
using System.Diagnostics;


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XnaButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using MouseButtons = System.Windows.Forms.MouseButtons;


namespace CeGui.Renderers.Xna.Source
{
    /// <summary>
    /// Takes Keyboard activity from XNA, and injects into CeGui#
    /// </summary>
    sealed class InputInjector
    {
#region Public Members

        /// <summary>
        /// call Instance to get the singleton
        /// </summary>
        public static readonly InputInjector Instance = new InputInjector();

        /// <summary>
        /// Keyboard autorepeat delay
        /// </summary>
        public static TimeSpan AutoRepeatDelay { get { return autoRepeatDelay; } set { autoRepeatDelay = value; } }

#endregion Public Members

#region Public Methods

        /// <summary>
        /// Reads keyboard activity from XNA, and injects into CeGui#
        /// </summary>
        public static void processKeyboardInput(GameTime gameTime)
        {
            KeyboardState newKeyboardState = Keyboard.GetState();
            Keys[] oldKeys = oldKeyboardState.GetPressedKeys();
            Keys[] newKeys = newKeyboardState.GetPressedKeys();

            // if no input, skip the rest
            if ((0 != oldKeys.Length) || (0 != newKeys.Length))
            {
                Keys[] addedKeys = findAdded(oldKeys, newKeys);
                if (addedKeys.Length != 0)
                {
                    doPressed(addedKeys, isShiftKeyDown(newKeyboardState));
                }
                else
                {
                    doAutoRepeat(isShiftKeyDown(newKeyboardState), gameTime);
                }
                doReleased(findAdded(newKeys, oldKeys));
                oldKeyboardState = newKeyboardState;
            }
        }

        /// <summary>
        /// Collect mouse movement from Xna and inject into CeGui
        /// </summary>
        public static void processMouseInput()
        {
            MouseState mouseState = Mouse.GetState();

            // only inject mouse activity if it's over the App's window
            bool inWindow = isMouseInAppWindow(mouseState);
            if (!inWindow)
            {
                // if mouse exited window, tell CeGui
                if (wasMouseInWindow)
                {
                    GuiSystem.Instance.InjectMouseLeaves();
                }
            }
            else
            {
                // Hand over the new mouse position to CeGui
                GuiSystem.Instance.InjectMousePosition(mouseState.X, mouseState.Y);


                // Determine whether the mouse wheel was moved and if so, inform CeGui
                if (mouseState.ScrollWheelValue != mouseWheelValue)
                {
                    GuiSystem.Instance.InjectMouseWheel(
                      mouseWheelValue - mouseState.ScrollWheelValue
                    );
                }

                // Left mouse button press/release notification
                if (mouseState.LeftButton != leftMouseButtonState)
                {
                    injectMouseUpDown(MouseButtons.Left, mouseState.LeftButton);
                }

                // Right mouse button press/release notification
                if (mouseState.RightButton != rightMouseButtonState)
                {
                    injectMouseUpDown(MouseButtons.Right, mouseState.RightButton);
                }
            }

            // track mouse activity for next time
            mouseWheelValue       = mouseState.ScrollWheelValue;
            leftMouseButtonState  = mouseState.LeftButton;
            rightMouseButtonState = mouseState.RightButton;
            wasMouseInWindow      = inWindow;
        }

#endregion Public Methods

#region Private Methods

        private InputInjector()
        {
        }

        /// <summary>
        /// Return true if one of the shift keys is down
        /// </summary>
        private static bool isShiftKeyDown(KeyboardState state)
        {
            return (state.IsKeyDown(Keys.LeftShift)|| state.IsKeyDown(Keys.RightShift));
        }

        /// <summary>
        /// Find the keys that have been added to the set
        /// </summary>
        /// <param name="baseKeys">The "first" set of keys to search/param>
        /// <param name="diffKeys">The set of keys to compare with the first set</param>
        /// <returns>Keys that are in diffKeys and not in baseKeys</returns>
        private static Keys[] findAdded(Keys[] baseKeys, Keys[] diffKeys)
        {
            List<Keys> added = new List<Keys>();
            foreach(Keys k in diffKeys)
            {
                if (!find(k, baseKeys))
                {
                    added.Add(k);
                }
            }
            return added.ToArray();
        }

        /// <summary>
        /// Search for key in keys
        /// </summary>
        /// <param name="key">key to look for</param>
        /// <param name="keys">Set of keys to look through</param>
        /// <returns>true if k is in keys</returns>
        private static bool find(Keys key, Keys[] keys)
        {
            for (int i = 0; i < keys.Length; ++i)
            {
                if (key == keys[i])
                {
                    return true;
                }
            }
            // if get here, k was not found
            return false;
        }
        
        /// <summary>
        /// Inform CeGui of the keys being pressed
        /// </summary>
        /// <param name="pressed">The keys that have been pressed since last time</param>
        /// <param name="shiftDown">Is shift key down (needed to capitalize characters)</param>
        private static void doPressed(Keys[] pressed, bool shiftDown)
        {
            if (pressed.Length != 0)
            {
                clearAutoRepeat();
                autoRepeatKeys.AddRange(pressed);
            }

            press(pressed, shiftDown);
        }

        private static void press(IEnumerable<Keys> pressed, bool shiftDown)
        {
            foreach (Keys k in pressed)
            {
                // abuse fact that Xna.Framework.Input.Keys values == System.Windows.Forms.Keys
                GuiSystem.Instance.InjectKeyDown((System.Windows.Forms.Keys)k);
                injectChar(k, shiftDown);
            }
        }

        /// <summary>
        /// Keyboard autorepeat
        /// </summary>
        /// <param name="shiftDown">Is shift key down</param>
        /// <param name="gameTime">Time elapsed since last call</param>
        private static void doAutoRepeat(bool shiftDown, GameTime gameTime)
        {
            autoRepeatTimeElapsed += gameTime.ElapsedGameTime;

            bool repeat = false;
            if (autoRepeatStarted)
            {
                repeat = autoRepeatTimeElapsed >= AutoRepeatDelay;
            }
            else
            {
                repeat = autoRepeatTimeElapsed.Ticks >= 20 * AutoRepeatDelay.Ticks;
            }

            if (repeat)
            {
                autoRepeatStarted = true;
                autoRepeatTimeElapsed = new TimeSpan(0);
                press(autoRepeatKeys, shiftDown);
            }
        }

        private static void clearAutoRepeat()
        {
            autoRepeatKeys.Clear();
            autoRepeatTimeElapsed = new TimeSpan(0);
            autoRepeatStarted = false;
        }

        /// <summary>
        /// Convert key into Unicode character, and inject into cegui
        /// </summary>
        /// <param name="k">Keycode</param>
        /// <param name="shiftDown">Is shift key down</param>
        private static void injectChar(Keys k, bool shiftDown)
        {
            char charValue = KeyToUnicode(k, shiftDown);
            if (charValue != 0)
            {
                GuiSystem.Instance.InjectChar(charValue);
            }
        }

        /// <summary>
        /// Convert Keycode into Unicode character
        /// </summary>
        /// <param name="k">Keycode</param>
        /// <param name="shiftDown">Is shift key down</param>
        private static char KeyToUnicode(Keys k, bool shiftDown)
        {
            // we're going to cheat a lot, and only process A to Z, Numpad 0 to 9, and space
            // and abuse the enum values in Microsoft.Xna.Framwork.Input.Keys
            int charValue = 0;

            if ((Keys.NumPad0 <= k) && (k <= Keys.NumPad9))
            {
                charValue = '0' + (k - Keys.NumPad0);
            }
            else if ((Keys.A <= k) && (k <= Keys.Z))
            {
                // cheat, k has ASCII value of key, which is same as Unicode
                if (shiftDown)
                {
                    charValue = (char)k;
                }
                else
                {
                    charValue = Char.ToLower((char)k);
                }
            }
            else
            {
                switch (k)
                {
                    case Keys.Space:
                        charValue = (char)k;
                        break;
                }
            }

            return (char)charValue;
        }

        /// <summary>
        /// Inform CeGui of the keys being released
        /// </summary>
        /// <param name="released">The keys that have been released since last time</param>
        private static void doReleased(Keys[] released)
        {
            if (released.Length != 0)
            {
                clearAutoRepeat();
            }
            foreach (Keys k in released)
            {
                // abuse fact that Xna.Framework.Input.Keys values == System.Windows.Forms.Keys
                GuiSystem.Instance.InjectKeyUp((System.Windows.Forms.Keys)k);
            }
        }

        /// <summary>Injects either a MouseDown or a MouseUp depending on the state</summary>
        /// <param name="button">Button to inject the event for</param>
        /// <param name="state">State of the button, decides which event is injected</param>
        private static void injectMouseUpDown(MouseButtons button, XnaButtonState state)
        {
            if (state == XnaButtonState.Pressed)
                GuiSystem.Instance.InjectMouseDown(button);
            else
                GuiSystem.Instance.InjectMouseUp(button);
        }

        /// <summary>
        /// Check if mouse cursor is currently inside the app's window
        /// </summary>
        /// <param name="state">current mouse info</param>
        /// <returns>True if mouse is inside app's window</returns>
        private static bool isMouseInAppWindow(MouseState mouseState)
        {
            PointF point = new PointF(mouseState.X, mouseState.Y);
            return GuiSystem.Instance.Renderer.Rect.IsPointInRect(point);
        }

#endregion Private Methods

#region Private Members

        /// <summary>
        /// The keys that were pressed last time inject() was called
        /// </summary>
        private static KeyboardState oldKeyboardState = Keyboard.GetState();

        /// <summary>Status of the left mouse button</summary>
        private static XnaButtonState leftMouseButtonState;
        /// <summary>Status of the right mouse button</summary>
        private static XnaButtonState rightMouseButtonState;
        /// <summary>Current value of the mouse scroll wheel</summary>
        private static int mouseWheelValue;

        /// <summary>Was the cursor inside the apps window the last time we checked?</summary>
        private static bool wasMouseInWindow = true;

        private static TimeSpan autoRepeatDelay = new TimeSpan(0, 0, 0, 0, 20);
        private static bool autoRepeatStarted = false;
        private static List<Keys> autoRepeatKeys = new List<Keys>();
        private static TimeSpan autoRepeatTimeElapsed = new TimeSpan(0);

#endregion Private Members
    }
}
