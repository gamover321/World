using System.Diagnostics;
using World.Engine.My2dWorld.Primitives;
using World.Engine.Primitives;

namespace World.Engine.My2dWorld;

public class My2dWorld
{
	private static int _stepPerSecond = 60;

	private TimeSpan _estimatedStepTime = TimeSpan.FromMilliseconds((double)1000 / _stepPerSecond);
	private TimeSpan _nextStepTime = TimeSpan.Zero;
	private TimeSpan _stepCalcTime = TimeSpan.Zero;

	private CancellationTokenSource _cancellationTokenSource;
	private Stopwatch _stepStopwatch = Stopwatch.StartNew();
	private Stopwatch _globalTimeStopwatch { get; set; } = new();

	public bool IsRunning { get; set; }
	public int CurrentStep { get; private set; } = 0;
	public TimeSpan WorldTime => _globalTimeStopwatch.Elapsed;


	public int Width { get; }
	public int Height { get; }

	private Vector.Vector2 Gravity { get; set; }
	private Dictionary<IPhysicsEntity, Dictionary<string, MyVector>> CustomForces { get; set; } = new();

	public List<PhysicsBaseEntity> Entities { get; set; }


	public My2dWorld(int width, int height)
	{
		Width = width;
		Height = height;

		Entities = new List<PhysicsBaseEntity>();

		/*
		for (var i = 0; i < 50; i++)
		{
		    var rect = new PhysicsRectangle(100, 100, 100f, 100f);
		    rect.SetSpeed(new MyVector.MyVector(0.5f+i*0.1f, -1));
		    rect.SetAngularSpeed(5f);

		    rect.SetAcceleration(new MyVector.MyVector(0, 0.01f));
		    rect.SetAngularAcceleration(-0.02f);

		    Entities.Add(rect);
		}
		*/

		var xl = new PhysicsRectangle(200, 300, 50, 50, 3)
		{
			Name = "x-left"
		};
		xl.Velocity = new MyVector(1, 0);

		var xr = new PhysicsRectangle(600, 470, 50, 50, 3)
		{
			Name = "x-right"
		};
		xr.Velocity = new MyVector(-1, 0);

		var y = new PhysicsRectangle(400, 390, 50, 150, 1);
		y.Name = "y";
		
		//y.Velocity = new MyVector(-1, 0);

		var z = new PhysicsRectangle(800, 300, 50, 50, 3);
		z.Name = "z";
		z.Velocity = new MyVector(-2, 0);

		var top = new PhysicsRectangle(330, 150, 30, 30, 1);
		top.Name = "top";
		top.Velocity = new MyVector(0, 0.7f);

		//x.AngularVelocity = (float)0.001;




		Entities.Add(xl);
		Entities.Add(xr);
		Entities.Add(y);
		Entities.Add(z);
		Entities.Add(top);

		//AddCustomForce("gravity",new MyVector.MyVector(0, 0.005f));
	}

	public void Start()
	{
		_cancellationTokenSource = new CancellationTokenSource();

		Task.Run(async () =>
		{
			try
			{
				_stepStopwatch = new Stopwatch();
				_stepStopwatch.Start();

				_globalTimeStopwatch = new Stopwatch();
				_globalTimeStopwatch.Start();

				IsRunning = true;
				while (!_cancellationTokenSource.Token.IsCancellationRequested)
				{
					var frameWasUpdated = Tick();
					if (frameWasUpdated)
					{
						CurrentStep++;
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}, _cancellationTokenSource.Token);
	}

	public void Stop()
	{
		_cancellationTokenSource.Cancel();
		_globalTimeStopwatch.Stop();

		IsRunning = false;
	}

	public void ManualUpdate()
	{
		var frameWasUpdated = Tick();
		if (frameWasUpdated)
		{
			CurrentStep++;
		}
	}

	public void AddCustomForce(string name, MyVector force, IPhysicsEntity? applyTo = null)
	{
		if (applyTo != null)
		{
			if (!CustomForces.ContainsKey(applyTo))
			{
				CustomForces.Add(applyTo, new Dictionary<string, MyVector>());
			}

			CustomForces[applyTo][name] = force;
		}
		else
		{
			foreach (var entity in Entities)
			{
				if (!CustomForces.ContainsKey(entity))
				{
					CustomForces.Add(entity, new Dictionary<string, MyVector>());
				}

				CustomForces[entity][name] = force;
			}
		}
	}

	public void RemoveCustomForce(string name, IPhysicsEntity? applyTo = null)
	{
		if (applyTo != null)
		{
			if (CustomForces.ContainsKey(applyTo) && CustomForces[applyTo].ContainsKey(name))
			{
				CustomForces[applyTo].Remove(name);

				applyTo.RemoveForce(name);
			}
		}

		foreach (var entity in Entities)
		{
			if (CustomForces.ContainsKey(entity) && CustomForces[entity].ContainsKey(name))
			{
				CustomForces[entity].Remove(name);
			}

			entity.RemoveForce(name);
		}
	}

	public void DoLogic()
	{
		if ((Control.ModifierKeys & Keys.Shift) != 0)
		{
		    AddCustomForce("up", new MyVector(0, -0.01f));
		    RemoveCustomForce("down");
		}

		if ((Control.ModifierKeys & Keys.Alt) != 0)
		{
		    AddCustomForce("down", new MyVector(0, 0.01f));
		    RemoveCustomForce("up");
		}
	}

	void Update(float dt)
	{
		// 1. Очистка сил
		foreach (var entity in Entities)
		{
			entity.Forces.Clear();
			//entity.ApplyGravity(); // Если есть
		}

		// 2. Обработка столкновений
		ResolveCollisions(dt);

		// 3. Интегрирование
		foreach (var entity in Entities)
		{
			if (entity.IsStatic) continue;

			// a = F/m
			var totalForce = MyVector.Empty();
			foreach (var force in entity.Forces.Values)
			{
				totalForce += force;
			}

			MyVector acceleration = totalForce * entity.InverseMass;
			entity.Velocity += acceleration * dt;
			entity.Position += entity.Velocity * dt;

			// Угловое ускорение
			float angularAcceleration = entity.Torques.Select(i=>i.Value).Sum() / entity.Inertia;
			entity.AngularVelocity += angularAcceleration * dt;
			entity.AngularPosition += entity.AngularVelocity * dt;
		}
	}

	public int GetFps()
	{
		return (int)(1000 / _stepCalcTime.TotalMilliseconds);
	}

	private void ResolveCollisions(float dt)
	{
		const float positionPercent = 0.01f; // Уменьшил для меньшего "засасывания"
		const float slop = 0.001f;         // Более точный допуск
		const float restitution = 0.0f;     // Небольшая упругость

		foreach (var (a, b) in GetCollidingPairs())
		{
			var collision = a.GetCollisionInfo(b);
			if (!collision.Intersects) continue;

			a.CollisionPoints.Add(collision.ContactPoint);

			var normal = collision.Normal;
			var penetration = collision.Penetration;
			var contact = collision.ContactPoint;

			// 1. Проверка направления нормали (должна быть от B к A)
			MyVector centerToContact = contact - b.Position;
			if (normal.Dot(centerToContact) < 0)
			{
				normal = new MyVector(-normal.X, -normal.Y);
			}

			// 2. Позиционная коррекция
			float totalCorrection = Math.Max(penetration - slop, 0f) * positionPercent;
			float massSum = 1f / (a.InverseMass + b.InverseMass);
			MyVector correction = normal * totalCorrection;

			if (!a.IsStatic) a.Position += correction * a.InverseMass * massSum;
			if (!b.IsStatic) b.Position -= correction * b.InverseMass * massSum;

			// 3. Расчет импульса
			MyVector ra = contact - a.Position;
			MyVector rb = contact - b.Position;

			MyVector va = a.Velocity + new MyVector(-a.AngularVelocity * ra.Y, a.AngularVelocity * ra.X);
			MyVector vb = b.Velocity + new MyVector(-b.AngularVelocity * rb.Y, b.AngularVelocity * rb.X);
			MyVector relativeVelocity = vb - va;

			float velocityAlongNormal = relativeVelocity.Dot(normal);
			if (velocityAlongNormal < 0) continue;

			// 4. Расчет импульса
			float raCrossN = ra.Cross(normal);
			float rbCrossN = rb.Cross(normal);
			float invMassSum = a.InverseMass + b.InverseMass +
							 (raCrossN * raCrossN) * a.InverseInertia +
							 (rbCrossN * rbCrossN) * b.InverseInertia;

			float j = -(1 + restitution) * velocityAlongNormal / invMassSum;
			MyVector impulse = normal * j;

			// 5. Применение импульсов
			if (!a.IsStatic)
			{
				a.Velocity -= impulse * a.InverseMass;
				a.AngularVelocity -= ra.Cross(impulse) * a.InverseInertia;
			}

			if (!b.IsStatic)
			{
				b.Velocity += impulse * b.InverseMass;
				b.AngularVelocity += rb.Cross(impulse) * b.InverseInertia;
			}
		}
	}

	private IEnumerable<(PhysicsBaseEntity, PhysicsBaseEntity)> GetCollidingPairs()
	{
		for (var i = 0; i < Entities.Count; i++)
		{
			for (var j = i + 1; j < Entities.Count; j++)
			{
				yield return (Entities[i], Entities[j]);
			}
		}
	}

	/// <summary>
	/// Обновить состояние мира, если пришло время
	/// </summary>
	/// <returns></returns>
	private bool Tick()
	{
		if (_nextStepTime <= _stepStopwatch.Elapsed)
		{
			_stepCalcTime = _stepStopwatch.Elapsed;
			_stepStopwatch.Restart();
                
			DoLogic();
			Update(1);

			var elapsed = _stepStopwatch.Elapsed;
			var remains = _estimatedStepTime - elapsed;

			_nextStepTime = elapsed + remains;

			return true;
		}
            
		return false;
	}
}