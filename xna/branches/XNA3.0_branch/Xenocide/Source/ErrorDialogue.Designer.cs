namespace ProjectXenocide
{
    partial class ErrorDialogue
    {
        // we should not be modifying this code ever again, so commenting this out shuts up the warning.
        /*
        /// <summary>
        /// Required designer variable.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields",
            Justification="FxCop false positive")]
        private System.ComponentModel.IContainer components = null;
        */

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters",
            Justification = "It's debugging code")]
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ErrorDialogue));
            this.txtExceptionText = new System.Windows.Forms.TextBox();
            this.btnCopyToClipboard = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtExceptionText
            // 
            this.txtExceptionText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtExceptionText.Location = new System.Drawing.Point(0, 0);
            this.txtExceptionText.Multiline = true;
            this.txtExceptionText.Name = "txtExceptionText";
            this.txtExceptionText.ReadOnly = true;
            this.txtExceptionText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtExceptionText.Size = new System.Drawing.Size(292, 244);
            this.txtExceptionText.TabIndex = 0;
            // 
            // btnCopyToClipboard
            // 
            this.btnCopyToClipboard.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnCopyToClipboard.Location = new System.Drawing.Point(0, 243);
            this.btnCopyToClipboard.Name = "btnCopyToClipboard";
            this.btnCopyToClipboard.Size = new System.Drawing.Size(292, 30);
            this.btnCopyToClipboard.TabIndex = 1;
            this.btnCopyToClipboard.Text = "Copy error text to clipboard";
            this.btnCopyToClipboard.UseVisualStyleBackColor = true;
            this.btnCopyToClipboard.Click += new System.EventHandler(this.btnCopyToClipboard_Click);
            // 
            // ErrorDialogue
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.btnCopyToClipboard);
            this.Controls.Add(this.txtExceptionText);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Name = "ErrorDialogue";
            this.Text = "An Error Occured";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtExceptionText;
        private System.Windows.Forms.Button btnCopyToClipboard;
    }
}
