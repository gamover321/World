using SkiaSharp;
using World.Engine.My2dWorld;
using World.Engine.My2dWorld.Primitives;
using World.Engine.Render.Primitives;

namespace World.Engine.Render;

public class WorldRender
{
	/// <summary>
	/// Кол-во желаемых кадров в секунду
	/// </summary>
	private static int _fps => 30;

	private float _accumulator;
	private float _fixedTimeStep;

	
	private TimeSpan _frameRenderTime = TimeSpan.Zero;

	private PhysicsWorld _world;
	private Camera2d _camera;
	private CancellationTokenSource _cancellationTokenSource;
	private SKImage CurrentFrame { get; set; }


	public bool IsPaused { get; private set; } = true;
	public Dictionary<PhysicsBaseEntity, BaseEntityRender> RenderDict = new();

	public WorldRender(PhysicsWorld world, Camera2d camera2d, int fps = 60)
	{
		_world = world;
		_camera = camera2d;
		_fixedTimeStep = 1f / fps;
	}

	public void UpdateWithVariableDelta(float deltaTime)
	{
		_accumulator += deltaTime;
		while (_accumulator>=_fixedTimeStep)
		{
			CurrentFrame = RenderWorld();
			_accumulator -= deltaTime;
		}
	}

	public void TogglePause()
	{
		IsPaused = !IsPaused;
	}

	public void SetPause(bool paused)
	{
		IsPaused = paused;
	}


	public void ManualUpdate()
	{
		CurrentFrame = RenderWorld();
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
			render.Render(canvas, _camera);
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