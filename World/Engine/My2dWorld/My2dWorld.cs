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
	private Stopwatch _stepStopwatch;
	private Stopwatch _globalTimeStopwatch { get; set; }

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

		var x = new PhysicsRectangle(200, 300, 50, 50, 500)
		{
			Name = "x"
		};

		var y = new PhysicsRectangle(400, 380, 50, 150, 1);
		y.Name = "y";

		var z = new PhysicsRectangle(800, 300, 50, 50, 1);
		z.Name = "z";

		var top = new PhysicsRectangle(400, 150, 30, 30, 1);
		top.Name = "top";

		x.Speed= new MyVector(1, 0);
		//x.AngularSpeed = (float)0.001;

		y.Speed = new MyVector(-1, 0);
		top.Speed = new MyVector(0, 2);

		Entities.Add(x);
		Entities.Add(y);
		//Entities.Add(z);
		//Entities.Add(top);

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

	public void Update()
	{
		foreach (var entity in Entities)
		{
			if (CustomForces.TryGetValue(entity, out var entityForces))
			{
				foreach (var force in entityForces)
					entity.AddForce(force.Key, force.Value);
			}

			entity.Update();


			entity.Forces.Clear();
			//entity.CollisionPoints.Clear();
		}

		ResolveCollisions();
	}

	private void ResolveCollisions()
	{
		for (var i = 0; i < Entities.Count; i++)
		{
			for (var j = i + 1; j < Entities.Count; j++)
			{
				var a = Entities[i];
				var b = Entities[j];

				var collisionInfo = b.GetCollisionInfo(a);
				if (!collisionInfo.Intersects)
				{
					continue;
				}

				a.CollisionPoints.Add(new MyVector(collisionInfo.ContactPoint.X, collisionInfo.ContactPoint.Y));

				var normal = collisionInfo.Normal;
				var penetration = collisionInfo.Penetration;
				var contactPoint = collisionInfo.ContactPoint;

				// Позиционная коррекция
				const float percent = 0.8f; // Насколько сильно корректируем
				const float slop = 0.01f;   // Минимальное смещение чтобы избежать "дрожания"
				const float maxCorrection = 20f;
				const float restitution = 0.2f;

				float correctionMagnitude = Math.Max(penetration - slop, 0f) / (a.InverseMass + b.InverseMass) * percent;
				correctionMagnitude = Math.Min(correctionMagnitude, maxCorrection);

				var correction = normal * correctionMagnitude;

				if (a.InverseMass > 0)
				{
					a.Position += correction * a.InverseMass;
				}

				if (b.InverseMass > 0)
				{
					b.Position -= correction * b.InverseMass;
				}

				var ra = new MyVector(contactPoint.X - a.Position.X, contactPoint.Y - a.Position.Y);
				var rb = new MyVector(contactPoint.X - b.Position.X, contactPoint.Y - b.Position.Y);

				var va = new MyVector(
					a.Speed.X - a.AngularSpeed * -ra.Y,
					a.Speed.Y + a.AngularSpeed * ra.X
				);
				var vb = new MyVector(
					b.Speed.X - b.AngularSpeed * -rb.Y,
					b.Speed.Y + b.AngularSpeed * rb.X
				);
				var relativeVelocity = new MyVector(va.X - vb.X, va.Y - vb.Y);

				var velAlongNormal = relativeVelocity.ScalarMul(normal);
				if (velAlongNormal > 0)
				{
					continue;
				}

				// Вращательное влияние на импульс
				var raCrossN = ra.X * normal.Y - ra.Y * normal.X;
				var rbCrossN = rb.X * normal.Y - rb.Y * normal.X;

				var denom = a.InverseMass + b.InverseMass +
				            (raCrossN * raCrossN) / a.Inertia +
				            (rbCrossN * rbCrossN) / b.Inertia;

				var jj = -(1 + restitution) * velAlongNormal / denom;

				var impulse = normal.Copy();
				impulse *=jj;

				var accelerationA = impulse * a.InverseMass;
				var accelerationB = impulse * -b.InverseMass;

				a.AddForce("contact", accelerationA);
				b.AddForce("contact", accelerationB);

				var angularAccelerationA = raCrossN * jj / a.Inertia;
				var angularAccelerationB = rbCrossN * jj / b.Inertia;

				a.AddTorque("contact", angularAccelerationA);
				b.AddTorque("contact", angularAccelerationB);
			}
		}
	}


	private MyVector EstimateContactPoint(PhysicsBaseEntity a, PhysicsBaseEntity b, MyVector normal)
	{
		var aVerts = a.GetTransformedEdges();
		var bVerts = b.GetTransformedEdges();

		var minDist = float.MaxValue;
		MyVector best = default;

		foreach (var va in aVerts)
		{
			foreach (var vb in bVerts)
			{
				var dist = (va.Sub(vb)).LengthSquared();
				if (dist < minDist)
				{
					minDist = dist;
					best = va.Add(vb).Mul((float)0.5);
				}
			}
		}

		return best;
	}


	public int GetFps()
	{
		return (int)(1000 / _stepCalcTime.TotalMilliseconds);
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
			Update();

			var elapsed = _stepStopwatch.Elapsed;
			var remains = _estimatedStepTime - elapsed;

			_nextStepTime = elapsed + remains;

			return true;
		}
            
		return false;
	}
}