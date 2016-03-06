using System;
using System.Collections.Generic;
using System.Text;
using ProjectXenocide.UI.Screens;

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// Work in progress (Rincewind)
    /// </summary>
    public interface IScreenManager
    {
        /// <summary>
        /// Work in progress
        /// </summary>
        /// <param name="newScreen"></param>
        void ScheduleScreen(Screen newScreen);

        /// <summary>
        /// Work in progress
        /// </summary>
        CeGui.GuiSheet RootGuiSheet { get; }
        /// <summary>
        /// Work in progress
        /// </summary>
        CeGui.GuiBuilder GuiBuilder { get; }
    }
}
