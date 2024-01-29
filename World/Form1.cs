using System.Diagnostics;
using System.Windows.Forms;
using SkiaSharp;
using World.Engine.My2dWorld;
using World.Engine.Render;
using World.Engine.Vector;


namespace World
{
    public partial class Form1 : Form
    {
        private My2dWorld _world;
        private Render _render;

        private bool _isRunning;



        public Form1()
        {
            InitializeComponent();

            _world = new My2dWorld(pictureBox1.Width, pictureBox1.Height);
            _render = new Render(_world);

            _render.Start();
            _isRunning = true;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            /*
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == MouseButtons.Left)
            {
                var random = new Random();
                var x = random.Next(-100, 100) / 100f;

                var force = new Vector(x, -2);
                _world.AddCustomForce(force);
            }

            if (me.Button == MouseButtons.Right)
            {
                var force = new Vector(0, 0);
                _world.AddCustomForce(force);
            }
            */
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            var image = _render.GetPicture();

            using (SKData data = image.Encode())
            using (System.IO.MemoryStream mStream = new System.IO.MemoryStream(data.ToArray()))
            {
                pictureBox1.Image?.Dispose();
                pictureBox1.Image = new Bitmap(mStream, false);
            }

            Text = PrepareCaption();

        }

        private string PrepareCaption()
        {
            var actualFps = _render.GetFps();
            var text = $"{actualFps:00}";

            return text;
        }



        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}