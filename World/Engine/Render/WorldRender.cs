using SkiaSharp;
using System.Diagnostics;
using World.Engine.My2dWorld.Primitives;
using World.Engine.Render.Primitives;

namespace World.Engine.Render
{
    public class WorldRender
    {
        /// <summary>
        /// Кол-во желаемых кадров в секунду
        /// </summary>
        private static int _fps => 30;

        private TimeSpan _frameTime = TimeSpan.FromMilliseconds((double)1000 / _fps);
        private TimeSpan _nextFrameTime = TimeSpan.Zero;
        private TimeSpan _frameRenderTime = TimeSpan.Zero;

        private My2dWorld.My2dWorld _world;
        private CancellationTokenSource _cancellationTokenSource;
        private Stopwatch _stopwatch;

        private SKImage CurrentFrame { get; set; }
       

        public Dictionary<PhysicsBaseEntity, BaseEntityRender> RenderDict = new();

        public WorldRender(My2dWorld.My2dWorld world)
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
                        _frameRenderTime = stopwatch.Elapsed;
                        stopwatch.Restart();
                    }
                }
            }, _cancellationTokenSource.Token);
        }

        public void ManualUpdate()
        {
            CurrentFrame = RenderWorld();
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }

        public SKImage GetPicture()
        {
            if (CurrentFrame == null)
            {
                CurrentFrame = RenderWorld();
            }
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

			foreach (var worldEntity in _world.Entities)
            {
                var render = GetRender(worldEntity);
                render.Render(canvas);
            }

            var image = surface.Snapshot();

            return image;
        }

        public BaseEntityRender GetRender(PhysicsBaseEntity entity)
        {
            if (!RenderDict.ContainsKey(entity))
            {
                RenderDict.Add(entity, new TrailEntityRender(entity));
            }

            return RenderDict[entity];
        }

        

        #endregion

    }
}
