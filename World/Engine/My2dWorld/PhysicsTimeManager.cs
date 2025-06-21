namespace World.Engine.My2dWorld;

public class PhysicsTimeManager // Управление временем
{
	private readonly PhysicsWorld _world;
	private float _accumulator;
	private float _fixedTimeStep;

	public bool IsPaused { get; private set; } = true;

	public PhysicsTimeManager(PhysicsWorld world, int updatesPerSecond = 60)
	{
		_world = world;
		_fixedTimeStep = 1f / updatesPerSecond;
	}

	public void UpdateWithVariableDelta(float deltaTime)
	{
		if (IsPaused)
		{
			return;
		}

		// Накопление времени
		_accumulator += deltaTime;

		// Fixed-update с фиксированным шагом
		while (_accumulator >= _fixedTimeStep)
		{
			_world.Update(_fixedTimeStep);
			_accumulator -= _fixedTimeStep;
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
}