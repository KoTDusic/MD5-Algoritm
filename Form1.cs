using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace md5
{
    public partial class Form1 : Form
    {
        md5 Coder;
        public Form1()
        {
            InitializeComponent();
            Coder = new md5(); 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            result_text.Text = Coder.GetHash(input_text.Text);
        }
    }
}
