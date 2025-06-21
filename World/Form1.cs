using System.Diagnostics;
using System.Windows.Forms;
using SkiaSharp;
using World.Engine.My2dWorld;
using World.Engine.My2dWorld.Primitives;
using World.Engine.Primitives;
using World.Engine.Render;
using World.Engine.Vector;
using Timer = System.Windows.Forms.Timer;


namespace World
{
	public partial class Form1 : Form
	{
		private PhysicsWorld _world;
		private WorldRender _worldRender;
		private readonly PhysicsTimeManager _timeManager;
		private readonly Timer _gameTimer;
		private float _speed = 1f;

		private bool _isRunning;

		public Form1()
		{
			InitializeComponent();

			_world = new PhysicsWorld(pictureBox1.Width, pictureBox1.Height);
			_timeManager = new PhysicsTimeManager(_world);

			_worldRender = new WorldRender(_world);

			_worldRender.Start();
			_isRunning = true;

			// Таймер для обновления игры
			_gameTimer = new Timer { Interval = 16 }; // ~60 FPS
			_gameTimer.Tick += GameLoop;
			_gameTimer.Start();
		}

		private void GameLoop(object sender, EventArgs e)
		{
			float deltaTime = _gameTimer.Interval / 1000f * _speed; // Конвертируем в секунды
			_timeManager.UpdateWithVariableDelta(deltaTime);
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
			acc = acc.Normalize();

			var mouseDist = entity.Position.DistanceTo(mouseVec);
			acc = acc.Mul((float)Math.Pow(0.000001f * mouseDist, 0.5));
			//acc.Mul(mul);

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

			//text += $" Step:{_world.CurrentStep} ({_world.GetFps()} fps - {_world.WorldTime.TotalSeconds:0} sec)";

			return text;
		}



		private void Form1_Load(object sender, EventArgs e)
		{

		}

		private void button1_Click(object sender, EventArgs e)
		{
			_world.Update(1);
		}

		private void button2_Click(object sender, EventArgs e)
		{
			_timeManager.TogglePause();
			button2.Text = _timeManager.IsPaused ? "Resume" : "Pause";
		}

		private void button3_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < 100; i++)
			{
				_world.Update(1);
			}

		}

		private void trackBar1_Scroll(object sender, EventArgs e)
		{
			TrackBar trackBar = (TrackBar)sender;
			_speed = trackBar.Value;
		}
	}
}