#region Copyright
/*
--------------------------------------------------------------------------------
This source file is part of Xenocide
  by  Project Xenocide Team

For the latest info on Xenocide, see http://www.projectxenocide.com/

This work is licensed under the Creative Commons
Attribution-NonCommercial-ShareAlike 2.5 License.

To view a copy of this license, visit
http://creativecommons.org/licenses/by-nc-sa/2.5/
or send a letter to Creative Commons, 543 Howard Street, 5th Floor,
San Francisco, California, 94105, USA.
--------------------------------------------------------------------------------
*/

/*
* @file Program.cs
* @date Created: 2007/01/20
* @author File creator: David Teviotdale
* @author Credits: XNA project wizard
*/
#endregion

using System;

using Microsoft.Xna.Framework.Graphics;

using ProjectXenocide.Utils;
using Xenocide.Resources;


namespace ProjectXenocide 
{
    static class Program 
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() 
        {
            // Set handler to catch exceptions
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);

            if (CheckGraphicsHardware())
            {
                using (Xenocide game = new Xenocide())
                {
                    game.Run();
                }
            }
        }

        /// <summary>Check that there's a graphics card that will do at least shader v1.1</summary>
        /// <returns>true if graphics hardware is adaquate</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions",
            Justification = "FxCop false positive")]
        private static bool CheckGraphicsHardware()
        {
            foreach (GraphicsAdapter adapter in GraphicsAdapter.Adapters)
            {
                if (adapter.IsDeviceTypeAvailable(DeviceType.Hardware) &&
                    (0 < Util.GetShaderVersion(adapter.GetCapabilities(DeviceType.Hardware))))
                {
                    return true;
                }
            }

            // no card found. Tell user and halt
            System.Windows.Forms.MessageBox.Show(Strings.EXCEPTION_NO_SHADER_CARD_FOUND);
            return false;
        }

        /// <summary>
        /// Trap any unhandled exceptions and give user hopefully useful error information
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions"), 
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", 
            MessageId = "System.Windows.Forms.MessageBox.Show(System.String)")]
        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
                Exception e = (Exception)args.ExceptionObject;
                ErrorDialogue errorDialogue = new ErrorDialogue(e);
                errorDialogue.ShowDialog();
                errorDialogue.Focus();
        }    
    }
}


