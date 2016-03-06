#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
#endregion

namespace Xenocide.GUISystem
{
    public interface IInputManagerService
    {
        event MouseHandler LeftClick;
        event MouseHandler RightClick;
        event MouseMoveHandler MouseMove;
    }

    public class MouseEvent
    {
        public MouseEvent(Point position, ButtonState leftButton, ButtonState rightButton)
        {
            this.position = position;
            this.leftButton = leftButton;
            this.rightButton = rightButton;
        }

        public Point Position { get { return position; } }
        public ButtonState LeftButton { get { return leftButton; } }
        public ButtonState RightButton { get { return rightButton; } }

        public Point position;
        private ButtonState leftButton;
        private ButtonState rightButton;
    }

    #region Delegate definitions
    public delegate void MouseHandler(object sender, MouseEvent e);
    public delegate void MouseMoveHandler(object sender, MouseEvent e);
    #endregion

    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class InputManager : Microsoft.Xna.Framework.GameComponent, IInputManagerService
    {
        public InputManager(Game game)
            : base(game)
        {
            Game.Services.AddService(typeof(IInputManagerService), this);
        }
        
        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            Mouse.WindowHandle = Game.Window.Handle;

            base.Initialize();
        }


        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();

            if (oldMouseState.X == mouseState.X
                && oldMouseState.Y == mouseState.Y)
            {
                if (oldMouseState.LeftButton == ButtonState.Pressed
                    && mouseState.LeftButton == ButtonState.Released)
                {
                    Debug.WriteLine("Left Click at " + mouseState.X + ":" + mouseState.Y);
                    if (LeftClick != null)
                        LeftClick(this, new MouseEvent(new Point(mouseState.X, mouseState.Y),
                                        mouseState.LeftButton, 
                                        mouseState.RightButton));
                }
                if (oldMouseState.RightButton == ButtonState.Pressed
                    && mouseState.RightButton == ButtonState.Released)
                {
                    if (RightClick != null)
                        RightClick(this, new MouseEvent(new Point(mouseState.X, mouseState.Y),
                                        mouseState.LeftButton, 
                                        mouseState.RightButton));
                }
            }
            else
            {
                if (MouseMove != null)
                    MouseMove(this, new MouseEvent(
                                        new Point(mouseState.X, mouseState.Y), 
                                        mouseState.LeftButton, 
                                        mouseState.RightButton));
            }

            oldMouseState = mouseState;

            base.Update(gameTime);
        }

        private MouseState oldMouseState;

        #region IInputManagerService Member

        public event MouseHandler LeftClick;
        public event MouseHandler RightClick;
        public event MouseMoveHandler MouseMove;

        #endregion
    }
}


