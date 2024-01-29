using SkiaSharp;
using System.Diagnostics;

namespace World.Engine.Render
{
    public class Render
    {
        /// <summary>
        /// Кол-во желаемых кадров в секунду
        /// </summary>
        private static int _fps => 60;

        private TimeSpan _frameTime = TimeSpan.FromMilliseconds((double)1000 / _fps);
        private TimeSpan _nextFrameTime = TimeSpan.Zero;
        private TimeSpan _frameRenderTime = TimeSpan.Zero;

        private My2dWorld.My2dWorld _world;
        private CancellationTokenSource _cancellationTokenSource;
        private Stopwatch _stopwatch;

        private SKImage CurrentFrame { get; set; }


        List<SKImage> PrevFrames = new List<SKImage>(20);

        public Render(My2dWorld.My2dWorld world)
        {
            _world = world;
            _stopwatch = new Stopwatch();
        }

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();


            Task.Run(async () =>
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    var frameWasUpdated = await Tick();
                    if (frameWasUpdated)
                    {
                        // Сохраняет пред. кадры
                        //if (PrevFrames.Count >= 20)
                        //{
                        //    PrevFrames.RemoveAt(0);
                        //}
                        //PrevFrames.Add(GetPicture());

                        _frameRenderTime = stopwatch.Elapsed;
                        stopwatch.Restart();
                    }
                }
            }, _cancellationTokenSource.Token);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }

        public SKImage GetPicture()
        {
            return CurrentFrame;
        }

        public int GetFps()
        {
            return (int)(1000 / _frameRenderTime.TotalMilliseconds);
        }

        #region Private methods

        private async Task<bool> Tick()
        {
            if (_nextFrameTime <= _stopwatch.Elapsed)
            {
                _stopwatch.Restart();

                _world.Update();

                CurrentFrame = RenderWorld();

                var elapsed = _stopwatch.Elapsed;
                var remains = _frameTime - elapsed;

                _nextFrameTime = _stopwatch.Elapsed + remains;

                return true;
            }

            return false;
        }

        private SKImage RenderWorld()
        {
            var imageInfo = new SKImageInfo(
                width: _world.Width,
                height: _world.Height,
                colorType: SKColorType.Rgba8888,
                alphaType: SKAlphaType.Premul);

            var surface = SKSurface.Create(imageInfo);

            var canvas = surface.Canvas;

            canvas.Clear(SKColor.Parse("#FFFFFF"));

            //foreach (var prevFrame in PrevFrames)
            //{
            //    canvas.Dr(prevFrame);
            //}

            var x1 = _world.Position.X1;
            var y1 = _world.Position.Y1;
            var x2 = _world.Position.X2;
            var y2 = _world.Position.Y2;
            var x3 = _world.Position.X3;
            var y3 = _world.Position.Y3;
            var x4 = _world.Position.X4;
            var y4 = _world.Position.Y4;

            canvas.DrawLine(x1, y1, x2, y2, new SKPaint{Color = SKColor.Parse("#000000")});
            canvas.DrawLine(x2, y2, x3, y3, new SKPaint { Color = SKColor.Parse("#000000") });
            canvas.DrawLine(x3, y3, x4, y4, new SKPaint { Color = SKColor.Parse("#000000") });
            canvas.DrawLine(x4, y4, x1, y1, new SKPaint { Color = SKColor.Parse("#000000") });

            /*canvas.DrawCircle(_world.Position.X, _world.Position.Y, 10, new SKPaint
            {
                Color = SKColor.Parse("##003366")
            });*/
            var image = surface.Snapshot();

            return image;
        }

        #endregion

    }
}
