using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ProjectXenocide.UI
{
    /// <summary>
    /// Reusable input handler that translates raw mouse state into scene-level events.
    /// Polls mouse state each frame, checks if the cursor is within a defined viewport
    /// rectangle, and fires callbacks with relative coordinates (0..1) when the user
    /// moves, clicks, or scrolls inside the scene area.
    ///
    /// This decouples the 3D scene (FacilityScene, PolarScene, etc.) from input
    /// handling so any screen with a 3D viewport can reuse the same hit-testing and
    /// edge-detection logic instead of duplicating it.
    ///
    /// ARCHITECTURE NOTE:
    /// - Screen-to-viewport projection: The UiRect passed to the constructor maps the scene's 3D
    ///   viewport onto the full screen using relative coordinates (0..1).  Each frame
    ///   we convert this to absolute pixels using the current GraphicsDevice.Viewport.
    /// - Edge detection:  We track previous frame's button state to fire events only
    ///   on the rising edge (press) not while held.  This prevents a single click
    ///   from generating multiple events.
    /// - Viewport-relative coords: Callbacks receive (relX, relY) in 0..1 space so
    ///   the scene can project them into world coordinates without knowing screen
    ///   resolution.  FacilityScene.WindowToCell() uses basic frustum trig to map
    ///   back to the 6x6 floorplan grid.
    /// - Leak prevention:  When a modal dialog closes (e.g. BuildFacilityDialog),
    ///   the mouse button may still be pressed.  The next frame's Update() would
    ///   see "leftDown && !prevLeftDown" and fire a spurious LeftClicked.  Call
    ///   Reset() after any state transition that follows a dialog to re-sync the
    ///   edge-detection baseline.
    ///
    /// Usage in a screen's Update():
    ///   sceneMouseHandler.Update();
    ///
    /// Usage in a screen's state transition (e.g. BasesScreen.State setter):
    ///   sceneMouseHandler?.Reset();
    /// </summary>
    public sealed class SceneMouseHandler
    {
        /// <summary>Fires every frame the cursor is inside the viewport, with relative coords.</summary>
        public event System.Action<float, float> MouseMoved;

        /// <summary>Fires on left-button press inside the viewport.</summary>
        public event System.Action<float, float> LeftClicked;

        /// <summary>Fires on right-button press inside the viewport.</summary>
        public event System.Action<float, float> RightClicked;

        /// <summary>Fires on scroll-wheel change inside the viewport.</summary>
        public event System.Action<int> ScrollWheelChanged;

        /// <param name="viewportRect">
        /// The scene area in relative screen coordinates (0..1).
        /// Typically matches the UiRect passed to the 3D scene's Draw().
        /// </param>
        public SceneMouseHandler(UiRect viewportRect)
        {
            this.viewportRect = viewportRect;
        }

        /// <summary>
        /// Call once per frame from the owning screen's Update().
        /// Polls mouse state, performs hit-testing, and fires events.
        /// </summary>
        public void Update()
        {
            Update(Mouse.GetState());
        }

        /// <summary>
        /// Resets the edge-detection state so the next Update() treats the current
        /// mouse state as the new baseline.  Call this after a dialog closes to
        /// prevent the button-down state from the dialog's "Select" click leaking
        /// into the scene as a spurious LeftClicked event.
        /// </summary>
        public void Reset()
        {
            MouseState current = Mouse.GetState();
            prevLeftDown = current.LeftButton == ButtonState.Pressed;
            prevRightDown = current.RightButton == ButtonState.Pressed;
            prevScrollValue = current.ScrollWheelValue;
        }

        /// <summary>
        /// Core update loop, extracted so Reset() can share the same logic
        /// without duplicating the viewport calculations.
        /// </summary>
        private void Update(MouseState mouse)
        {
            GraphicsDevice device = Xenocide.Instance.GraphicsDevice;

            // Convert relative viewport rect to absolute pixel coordinates
            int vpX = (int)(device.Viewport.Width * viewportRect.Left);
            int vpY = (int)(device.Viewport.Height * viewportRect.Top);
            int vpW = (int)(device.Viewport.Width * viewportRect.Width);
            int vpH = (int)(device.Viewport.Height * viewportRect.Height);

            bool inViewport = mouse.X >= vpX && mouse.X < vpX + vpW
                           && mouse.Y >= vpY && mouse.Y < vpY + vpH;

            // Fire MouseMoved every frame cursor is in the viewport
            if (inViewport)
            {
                float relX = (mouse.X - vpX) / (float)vpW;
                float relY = (mouse.Y - vpY) / (float)vpH;
                MouseMoved?.Invoke(relX, relY);
            }

            // Fire LeftClicked on edge-detect (button just pressed)
            bool leftDown = mouse.LeftButton == ButtonState.Pressed;
            if (leftDown && !prevLeftDown && inViewport)
            {
                float relX = (mouse.X - vpX) / (float)vpW;
                float relY = (mouse.Y - vpY) / (float)vpH;
                LeftClicked?.Invoke(relX, relY);
            }
            prevLeftDown = leftDown;

            // Fire RightClicked on edge-detect
            bool rightDown = mouse.RightButton == ButtonState.Pressed;
            if (rightDown && !prevRightDown && inViewport)
            {
                float relX = (mouse.X - vpX) / (float)vpW;
                float relY = (mouse.Y - vpY) / (float)vpH;
                RightClicked?.Invoke(relX, relY);
            }
            prevRightDown = rightDown;

            // Fire ScrollWheelChanged if wheel moved while in viewport
            int wheelDelta = mouse.ScrollWheelValue - prevScrollValue;
            if (inViewport && wheelDelta != 0)
            {
                ScrollWheelChanged?.Invoke(wheelDelta);
            }
            prevScrollValue = mouse.ScrollWheelValue;
        }

        private readonly UiRect viewportRect;
        private bool prevLeftDown;
        private bool prevRightDown;
        private int prevScrollValue;
    }
}
