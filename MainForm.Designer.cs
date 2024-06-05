namespace FormsSweeper
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            Icon = Properties.Resources.icon;
            // 
            // MainForm
            // 
            BackColor = Color.FromArgb(255 << 24 | 192 << 16 | 192 << 8 | 192);

            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(800, 450);
            Font = new Font("Microsoft Sans Serif", 16F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Minesweeper";
            Text = "Minesweeper";
            ResumeLayout(false);
        }

        #endregion
    }
}
