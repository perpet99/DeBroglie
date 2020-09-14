using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeBroglie.Console.Export;

namespace DeBroglie.Win
{
    public partial class Form1 : Form
    {

        //System.Windows.Forms.Button theControl;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            Pro = ItemsProcessor.Process("grass/map.json");

            Pro.ProcessItem();

            //pro.Run();

            TiledExportOptions teo = Pro.SampleSet.ExportOptions as TiledExportOptions;

            if (teo != null)
            {
                var basePath = Path.GetDirectoryName(teo.SrcFileName);

                pictureBox2.Image = Image.FromFile(Path.Combine(basePath, teo.Template.Tilesets[0].ImagePath));
            }

        }

        ItemsProcessor Pro;
        private Thread thread2 = null;


        private void button1_Click(object sender, EventArgs e)
        {
            thread2 = new Thread(new ThreadStart(SetText));
            thread2.Start();
            //Thread.Sleep(1000);
        }

        private async void SetText()
        {
            //WriteTextSafe("This text was set safely.");

          

            await Task.Run(() =>
            {


                Pro.Run2((String path) =>

                {

                    BeginInvoke((Action)(() =>
                    {
                        //button1.Text = path;
                        pictureBox1.Image = Image.FromFile(path);
                        pictureBox1.Update();

                    }));

                }
                    );

                

                //for (var i = 0; i <= 5000000; i++)
                //{
                //    count = i;
                //    BeginInvoke((Action)(() =>
                //    {
                //        button1.Text = i.ToString();

                //    }));
                //    Thread.Sleep(100);
                //}
            });


            BeginInvoke((Action)(() =>
            {
                button1.Text = "Done";

            }));
            




            //pictureBox1.Image = Image.FromFile(pro.Dest);

        }
        private void WriteTextSafe(string text)
        {
           
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {


            MouseEventArgs me = (MouseEventArgs)e;
            var pos = me.Location;

            pos.X -= pos.X % 16;
            pos.Y -= pos.Y % 16;

            //float stretch1X = 1f * pictureBox1.Image.Width / pictureBox1.ClientSize.Width;
            //float stretch1Y = 1f * pictureBox1.Image.Height / pictureBox1.ClientSize.Height;

            //var pt = new System.Drawing.Point((int)(mDown.X * stretch1X), (int)(mDown.Y * stretch1Y));
            //Size sz = new Size((int)((mCurr.X - mDown.X) * stretch1X),
            //                   (int)((mCurr.Y - mDown.Y) * stretch1Y));

            

            Rectangle rSrc = new Rectangle(pos, new Size(16,16));
            Rectangle rDest = new Rectangle(System.Drawing.Point.Empty, new Size(pictureBox3.ClientSize.Width, pictureBox3.ClientSize.Height));

            Bitmap bmp = new Bitmap(pictureBox3.ClientSize.Width, pictureBox3.ClientSize.Height);
            using (Graphics G = Graphics.FromImage(bmp))
                G.DrawImage(pictureBox2.Image, rDest, rSrc, GraphicsUnit.Pixel);
            pictureBox3.Image = bmp;

        }

        private void pictureBox2_MouseHover(object sender, EventArgs e)
        {
           

        }

        Rectangle Rect = new Rectangle();
        System.Drawing.Point mDown2;
        System.Drawing.Point mDown1;
        System.Drawing.Point mCurr;
        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            //MouseEventArgs me = (MouseEventArgs)e;
            //var coordinates = me.Location;
            mDown2 = e.Location;

            textBox1.Text = $"X : {e.Location.X} , Y : {e.Location.Y}";

            pictureBox2.Invalidate();

            //textBox1.Text = me.Location.X.ToString();
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {

            mDown2.X -= mDown2.X % 16;
            mDown2.Y -= mDown2.Y % 16;

            Rectangle r = new Rectangle(mDown2.X, mDown2.Y, 16, 16);
            e.Graphics.DrawRectangle(Pens.Red, r);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            mDown1 = e.Location;


            pictureBox1.Invalidate();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            mDown1.X -= mDown1.X % 16;
            mDown1.Y -= mDown1.Y % 16;

            Rectangle r = new Rectangle(mDown1.X, mDown1.Y, 16, 16);
            e.Graphics.DrawRectangle(Pens.Red, r);
        }
    }
}
