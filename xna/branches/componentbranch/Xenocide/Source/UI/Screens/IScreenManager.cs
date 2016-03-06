using System;
using System.Collections.Generic;
using System.Text;
using Xenocide.UI.Screens;
using Xenocide.UI.Dialogs;
using Microsoft.Xna.Framework.Content;

namespace Xenocide.UI.Screens
{
    /// <summary>
    /// Work in progress (Rincewind)
    /// </summary>
    public interface IScreenManager
    {
        /// <summary>
        /// Set the screen to show on the next update()
        /// </summary>
        /// <param name="newScreen">The new screen to show</param>
        /// <remarks>Need to delay swapping the screen until the next update.
        /// Due to issues with any dialogs being shown being "owned" by the screen
        /// currently being shown.
        /// </remarks>
        void ScheduleScreen(Screen newScreen);

        /// <summary>
        /// Put dialog into top of stack of dialogs being shown
        /// </summary>
        /// <param name="dialog">The dialog to show</param>
        void ShowDialog(Dialog dialog);

        /// <summary>
        /// Remove the topmost dialog currently being shown
        /// </summary>
        /// <param name="dialog">The dialog making the call (which should ALSO be the topmost dialog)</param>
        void CloseDialog(Dialog dialog);

        /// <summary>
        /// Put dialog into the queue to be displayed
        /// </summary>
        /// <param name="dialog">The dialog to queue</param>
        void QueueDialog(Dialog dialog);

        /// <summary>
        /// Retreive the Gui Sheet that is the root node of the tree of all CeGui#
        /// windows on the display
        /// </summary>
        /// <returns>the Gui Sheet</returns>
        /// RK:  BTW use a property named Sheet instead.
        /// DT: Except the Gui Sheet ISN'T owned by the ScreenManager.
        CeGui.GuiSheet RootGuiSheet { get; }

        /// <summary>
        /// The CeGui gui builder used to create widgets (that we later attach to screens/dialogs)
        /// </summary>
        CeGui.GuiBuilder GuiBuilder { get; }

        /// <summary>
        /// The central content manager to be used by all screens and dialogs
        /// </summary>
        ContentManager ContentManager { get; }

        /// <summary>
        /// set this to exit the game
        /// </summary>
        bool QuitGame { get; set; }
    }
}
