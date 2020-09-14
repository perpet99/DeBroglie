using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeBroglie.Win
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var pro = ItemsProcessor.Process("grass/map.json");

            pro.ProcessItem();

            pro.Run();

            pro.Run2((String path) =>

            {

            }
                );





            pictureBox1.Image = Image.FromFile(pro.Dest);

        }
    }
}
