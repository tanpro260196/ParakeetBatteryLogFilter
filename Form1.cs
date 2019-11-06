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
        }
        public DialogResult Show(string folderpath, string filename)
        {
            return InitializeComponent(folderpath, filename);
        }
    }
}
