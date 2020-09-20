using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeBroglie.Console.Export;

namespace DeBroglie.Win
{

    //public static class Util
    //{
    //    public static Stream ToStream(this Image image, ImageFormat format)
    //    {
    //        var stream = new System.IO.MemoryStream();
    //        image.Save(stream, format);
    //        stream.Position = 0;
    //        return stream;
    //    }

    //}

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

                pictureBox2.Image = FromImage(Path.Combine(basePath, teo.Template.Tilesets[0].ImagePath));
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
                        pictureBox1.Image = FromImage(path);
                        pictureBox1.Update();

                    }));

                });

                

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

        int SelectedPatten = 0;
        private void pictureBox2_Click(object sender, EventArgs e)
        {


            MouseEventArgs me = (MouseEventArgs)e;
            var pos = me.Location;

            SelectedPatten = pos.X / 16 + (pos.Y / 16) * 12;

            SelectedPatten++;

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

        System.Drawing.Point mDown1;
        System.Drawing.Point mDown2;
        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            //MouseEventArgs me = (MouseEventArgs)e;
            //var coordinates = me.Location;
            mDown2 = e.Location;

            int index = e.Location.X / 16 + e.Location.Y / 16 * 12;

            textBox1.Text = $"INDEX = {index + 1}";

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
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            var pos = me.Location;

            pos.X -= pos.X % 16;
            pos.Y -= pos.Y % 16;

            int x = pos.X / 16;
            int y = pos.Y / 16;

            //Pro.Propagator.SeletctPattern(x + y * 20, 1);

            //Pro.Export("test.png");

            //pictureBox1.Image = FromImage("test.png");
            //pictureBox1.Update();



            //var pro = ItemsProcessor.Process("grass/map2.json");

            //pro.ProcessItem();


            //pro.Propagator.Select()
            Pro.Propagator.Select(x , y , 0 ,new Tile( SelectedPatten));

            for (int sy = 0; sy < 5; sy++)
            {
                for (int sx = 0; sx < 5; sx++)
                {
                    if (sx == 0 || sx == 4 || sy == 0 || sy == 4)
                    {
                        int i = sx + x - 2 + (sy + y - 2) * 20;

                        var pattern = Pro.Propagator.GetTile(i);
                        //pro.Propagator.SeletctPattern(sx + sy * 5, pattern);
                        System.Diagnostics.Debug.WriteLine($"I : {i} ,pattern : {pattern}");
                        //Pro.Propagator.Select(sx + 1, sy + 1, 0, pattern);

                    }
                    else if (sx == 3 && sy == 3)
                    {

                    }
                    else
                    {
                        //model.Clear(sx, sy);
                    }
                }
            }

            //pro.Propagator.SetStatus(Resolution.Undecided);

            Pro.Run2((String path) =>

            {
                pictureBox1.Image = FromImage(path);
                pictureBox1.Update();


                //BeginInvoke((Action)(() =>
                //{
                //    //button1.Text = path;

                //}));

            });



        }
        private void pictureBox1_Click2(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            var pos = me.Location;

            pos.X -= pos.X % 16;
            pos.Y -= pos.Y % 16;

            int x = pos.X / 16;
            int y = pos.Y / 16;

            //Pro.Propagator.SeletctPattern(x + y * 20, 1);

            //Pro.Export("test.png");

            //pictureBox1.Image = FromImage("test.png");
            //pictureBox1.Update();



            var pro = ItemsProcessor.Process("grass/map2.json");

            pro.ProcessItem();


            //pro.Propagator.Select()
            //pro.Propagator.Select(3 , 3 , 0 ,new Tile( SelectedPatten));

            for (int sy = 0; sy < 5; sy++)
            {
                for (int sx = 0; sx < 5; sx++)
                {
                    if (sx == 0 || sx == 4 || sy == 0 || sy == 4)
                    {
                        int i = sx + x - 2 + (sy + y - 2) * 20;

                        var pattern = Pro.Propagator.GetTile(i);
                        //pro.Propagator.SeletctPattern(sx + sy * 5, pattern);
                        System.Diagnostics.Debug.WriteLine($"I : {i} ,pattern : {pattern}");
                        pro.Propagator.Select(sx+1, sy+1, 0, pattern);

                    }
                    else if (sx == 3 && sy == 3)
                    {
                        
                    }
                    else
                    {
                        //model.Clear(sx, sy);
                    }
                }
            }

            pro.Propagator.SetStatus( Resolution.Undecided);

            pro.Run2((String path) =>

            {
                pictureBox4.Image = FromImage(path);
                pictureBox4.Update();


                //BeginInvoke((Action)(() =>
                //{
                //    //button1.Text = path;

                //}));

            });



        }
        private void button2_Click(object sender, EventArgs e)
        {
            
        }

        public static Image FromImage(string path)
        {
            MemoryStream ms = new MemoryStream();
            using (var i = File.OpenRead(path))
            {
                i.CopyTo(ms);
            }
            return Image.FromStream(ms);
        }

    }


}
