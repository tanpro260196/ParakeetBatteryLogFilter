using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ParakeetBatteryLogFilter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public Form1(string title, string content, string filepath)
        {
            InitializeComponent();
            this.Text = title;
            this.label1.Text = content;
            this.AcceptButton = this.Open;
            this.CancelButton = this.Closes;
            this.Open.Click += (sender, e) => { System.Diagnostics.Process.Start(@filepath); Close(); };
            //this.Open.Enter += (sender, e) => { System.Diagnostics.Process.Start(@filepath); Close(); };
            this.Closes.Click += (sender, e) => { Close(); };
            this.Closes.Enter += (sender, e) => { Close(); };
        }
    }
}
