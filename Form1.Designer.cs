using System;
using System.Windows.Forms;

namespace ParakeetBatteryLogFilter
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private DialogResult InitializeComponent(string folderpath, string filename)
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.TopMost = true;
            string message = "Data exported to " + folderpath + "\\" + filename.Remove(filename.Length - 4) + ".csv." + Environment.NewLine + Environment.NewLine + "Open exported file?";
            string caption = "Success!";
            MessageBoxManager.Yes = "Open";
            MessageBoxManager.No = "Close";
            MessageBoxManager.Register();
            DialogResult result;
            result = MessageBox.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
            return result;
        }

        #endregion
    }
}