using System.Diagnostics;
using System.Windows.Forms;
using SkiaSharp;
using World.Engine.My2dWorld;
using World.Engine.My2dWorld.Primitives;
using World.Engine.Primitives;
using World.Engine.Render;
using World.Engine.Vector;


namespace World
{
    public partial class Form1 : Form
    {
        private My2dWorld _world;
        private WorldRender _worldRender;

        private bool _isRunning;
		
        public Form1()
        {
            InitializeComponent();

            _world = new My2dWorld(pictureBox1.Width, pictureBox1.Height);
            _world.Start();

            _worldRender = new WorldRender(_world);

            _worldRender.Start();
            _isRunning = true;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            
            if (_isRunning)
            {
                //_world.Stop();
                _isRunning = false;
            }
            else
            {
                //_world.Start();
               _isRunning = true;
            }
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var p = pictureBox1.PointToClient(Cursor.Position);
            var mouseVec = new MyVector(p.X, p.Y);

            foreach (var entity in _world.Entities)
            {
                MoveToCursor(mouseVec, entity);
            }

            var image = _worldRender.GetPicture();

            using (SKData data = image.Encode())
            using (System.IO.MemoryStream mStream = new System.IO.MemoryStream(data.ToArray()))
            {
                pictureBox1.Image?.Dispose();
                pictureBox1.Image = new Bitmap(mStream, false);
            }

            Text = PrepareCaption();
        }

        private void MoveToCursor(MyVector mouseVec, PhysicsBaseEntity entity, float mul = 1)
        {
            var relX = mouseVec.X - entity.Position.X;
            var relY = mouseVec.Y - entity.Position.Y;

            var acc = new MyVector(relX, relY);
            acc.Normalize();

            var mouseDist = entity.Position.DistanceTo(mouseVec);
            acc.Mul((float)Math.Pow(0.00001f * mouseDist, 0.5));
            acc.Mul(mul);

            if (!_isRunning)
            {
                _world.AddCustomForce("cursor_force", acc, entity);
            }
            else
            {
                _world.RemoveCustomForce("cursor_force");
            }

            //entity.SetAngularAcceleration(acc.X);
        }

        private string PrepareCaption()
        {
            var actualFps = _worldRender.GetFps();
            var text = $"{actualFps:00}";

            var p = pictureBox1.PointToClient(Cursor.Position);

            text += $" ({p.X}, {p.Y})";

            text += $" W:{_world.CurrentStep} ({_world.GetFps()} - {_world.WorldTime.TotalSeconds:0})";

            return text;
        }



        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}