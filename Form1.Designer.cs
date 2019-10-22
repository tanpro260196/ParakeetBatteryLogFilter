using System;
using System.Drawing;
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
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.Open = new System.Windows.Forms.Button();
            this.Closes = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Open
            // 
            this.Open.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.Open.Location = new System.Drawing.Point(9, 88);
            this.Open.Name = "Open";
            this.Open.Size = new System.Drawing.Size(75, 23);
            this.Open.TabIndex = 0;
            this.Open.Text = "Open";
            // 
            // Close
            // 
            this.Closes.DialogResult = System.Windows.Forms.DialogResult.No;
            this.Closes.Location = new System.Drawing.Point(140, 88);
            this.Closes.Name = "Close";
            this.Closes.Size = new System.Drawing.Size(75, 23);
            this.Closes.TabIndex = 1;
            this.Closes.Text = "Close";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "label1";
            // 
            // Form1
            // 
            this.AcceptButton = this.Open;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.Closes;
            this.ClientSize = new System.Drawing.Size(227, 123);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Closes);
            this.Controls.Add(this.Open);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Success!";
            this.ResumeLayout(false);

        }

        #endregion

        private Button Open;
        private Button Closes;
        private Label label1;
    }
}