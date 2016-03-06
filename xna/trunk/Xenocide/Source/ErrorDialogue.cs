using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ProjectXenocide
{
    /// <summary>
    /// Dialogue that allows copying of exception text to clipboard
    /// </summary>
    public partial class ErrorDialogue : Form
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exception">exception that caused this dialogue to show</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters",
            Justification = "It's debugging code")]
        public ErrorDialogue(Exception exception)
            : this()
        {
            Cursor.Show();
            if (null != exception)
            {
                string message = "Exception: " + exception.Message + "\r\nStackTrace:\r\n" + exception.StackTrace;
                this.txtExceptionText.Text = message;
            }
        }

        /// <summary>
        /// Hidden default constructor
        /// </summary>
        private ErrorDialogue()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Event handler for copy to clipboard button click
        /// </summary>
        /// <param name="sender">object originating event</param>
        /// <param name="e">event args</param>
        private void btnCopyToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(this.txtExceptionText.Text, true);
        }
    }
}
