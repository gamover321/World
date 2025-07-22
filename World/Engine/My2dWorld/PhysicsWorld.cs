using World.Engine.My2dWorld.Primitives;
using World.Engine.Primitives;

namespace World.Engine.My2dWorld;

public class PhysicsWorld
{
	public int CurrentStep { get; private set; }
	public int Width { get; }
	public int Height { get; }

	public List<PhysicsBaseEntity> Entities { get; set; }

	private Vector.Vector2 Gravity { get; set; }
	private Dictionary<IPhysicsEntity, Dictionary<string, MyVector>> CustomForces { get; set; } = new();


	public PhysicsWorld(int width, int height)
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

		
		var xl = new PhysicsRectangle(200, 300, 50, 50, 30)
		{
			Name = "x-left"
		};
		//xl.Velocity = new MyVector(1, 0);

		var xr = new PhysicsRectangle(600, 470, 50, 50, 30)
		{
			Name = "x-right"
		};
		//xr.Velocity = new MyVector(-1, 0);

		var y = new PhysicsRectangle(500, 470, 50, 50, 30);
		y.Name = "y";
		
		//y.Velocity = new MyVector(-1, 0);

		var z = new PhysicsRectangle(800, 300, 50, 50, 30);
		z.Name = "z";
		//z.Velocity = new MyVector(-2, 0);

		var top = new PhysicsRectangle(330, 150, 30, 30, 10);
		top.Name = "top";
		//top.Velocity = new MyVector(0, 0.7f);

		//x.AngularVelocity = (float)0.001;

		for (var i = 0; i < 10; i++)
		{
			var bullet = new PhysicsRectangle(50+30*i, 600, 25, 25, 1);
			bullet.Name = $"bullet-{i}";
			bullet.Velocity = new MyVector(5+i, -4f+i*0.2f);

			//Entities.Add(bullet);
		}

		var bottom = new PhysicsRectangle(-200, 700, 3500, 50, 0);
		bottom.Name = "bottom";
		Entities.Add(bottom);

		var pentagon = new PhysicsStar(600, 400, 100, 100, 1);
		Entities.Add(pentagon);

		//Entities.Add(xl);
		Entities.Add(xr);
		Entities.Add(y);
		//Entities.Add(z);
		//Entities.Add(top);
		
		

		AddCustomForce("gravity", new MyVector(0, 1.02f));


		/*
		var floor = new PhysicsRectangle(400, 650, 2000, 50, 0);
		var leftWall = new PhysicsRectangle(0, 300, 50, 2000, 0);
		var rightWall = new PhysicsRectangle(1000, 300, 50, 2000, 0);

		var rect1 = new PhysicsRectangle(375, 50, 50, 50, 1);
		rect1.Velocity = new MyVector(0, -1f);

		var rect2 = new PhysicsRectangle(400, 145, 50, 50, 1);
		rect2.Velocity = new MyVector(0, -2f);

		Entities.Add(floor);
		Entities.Add(leftWall);
		Entities.Add(rightWall);

		Entities.Add(rect1);
		Entities.Add(rect2);

		AddCustomForce("gravity", new MyVector(0, 0.01f));
		*/
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

	public void Update(float dt)
	{
		// 1. Предварительное интегрирование и установка желаемого положения
		foreach (var entity in Entities)
		{
			if (entity.IsStatic)
			{
				continue;
			}

			// a = F/m
			if (CustomForces.TryGetValue(entity, out var customForces))
			{
				foreach (var customForce in customForces)
				{
					entity.AddForce(customForce.Value, customForce.Key);
				}
			}
			
			entity.Acceleration = entity.GetTotalAcceleration();
			entity.Velocity += entity.Acceleration * dt;

			// Угловое ускорение
			var torque = entity.GetTotalTorque();
			entity.AngularAcceleration = torque;
			entity.AngularVelocity += torque * dt;
			

			entity.PredictedPosition += entity.Velocity * dt;
			entity.PredictedAngle += entity.AngularVelocity * dt;
		}

		// 2. Очистка сил
		foreach (var entity in Entities)
		{
			entity.ClearAllForces();
			entity.ClearTorque();
			//entity.ApplyGravity(); // Если есть
		}


		// 3. Обработка столкновений
		ResolveCollisions(dt);

		//4. Установка реаального положения
		foreach (var entity in Entities)
		{
			entity.Position = entity.PredictedPosition;
			entity.Angle = entity.PredictedAngle;
		}

		CurrentStep++;
	}

	private static float SumTorques(PhysicsBaseEntity entity)
	{
		var angularAcceleration = 0f;
		foreach (var torque in entity.Torques.Values)
		{
			angularAcceleration += torque;
		}
		
		return angularAcceleration;
	}

	private MyVector SumForces(PhysicsBaseEntity entity)
	{
		var totalForce = MyVector.Empty();
		foreach (var force in entity.Forces.Values)
		{
			totalForce += force;
		}

		if (CustomForces.TryGetValue(entity, out var entityCustomForces))
		{
			foreach (var force in entityCustomForces.Values)
			{
				totalForce += force;
			}
		}

		return totalForce;
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

	private void ResolveCollisions(float dt)
	{
		const float positionPercent = 1f;
		const float slop = 0.001f;
		const float restitution = 0.1f;

		foreach (var (a, b) in GetCollidingPairs())
		{
			var collision = a.GetCollisionInfo(b, PhysicsContext.Predicted);
			if (!collision.Intersects)
			{
				continue;
			}

			a.CollisionPoints.Add(collision.ContactPoint);

			var normal = collision.Normal;
			var penetration = collision.Penetration;
			var contact = collision.ContactPoint;

			// 1. Определяем, кто статический
			bool aIsStatic = a.IsStatic;
			bool bIsStatic = b.IsStatic;

			// 2. Позиционная коррекция (только для подвижных объектов)
			if (!aIsStatic || !bIsStatic)
			{
				float totalCorrection = Math.Max(penetration - slop, 0f) * positionPercent;
				float massSum = aIsStatic ? 1f / b.InverseMass :
							  bIsStatic ? 1f / a.InverseMass :
							  1f / (a.InverseMass + b.InverseMass);

				MyVector correction = normal * totalCorrection;

				if (!aIsStatic) a.PredictedPosition -= correction * (bIsStatic ? 1f : a.InverseMass * massSum);
				if (!bIsStatic) b.PredictedPosition += correction * (aIsStatic ? 1f : b.InverseMass * massSum);
			}

			// 3. Расчёт импульса (пропускаем если оба статичны)
			if (aIsStatic && bIsStatic) continue;

			MyVector ra = contact - a.GetPosition(PhysicsContext.Predicted);
			MyVector rb = contact - b.GetPosition(PhysicsContext.Predicted);

			MyVector va = aIsStatic ? MyVector.Empty() :
						 a.Velocity + new MyVector(-a.AngularVelocity * ra.Y, a.AngularVelocity * ra.X);

			MyVector vb = bIsStatic ? MyVector.Empty() :
						 b.Velocity + new MyVector(-b.AngularVelocity * rb.Y, b.AngularVelocity * rb.X);

			MyVector relativeVelocity = vb - va;
			float velocityAlongNormal = relativeVelocity.Dot(normal);
			if (velocityAlongNormal > 0) continue;

			// 4. Учёт статичности в расчёте импульса
			float invMassA = aIsStatic ? 0f : a.InverseMass;
			float invMassB = bIsStatic ? 0f : b.InverseMass;
			float invInertiaA = aIsStatic ? 0f : a.InverseInertia;
			float invInertiaB = bIsStatic ? 0f : b.InverseInertia;

			float raCrossN = aIsStatic ? 0f : ra.Cross(normal);
			float rbCrossN = bIsStatic ? 0f : rb.Cross(normal);

			float invMassSum = invMassA + invMassB +
							 raCrossN * raCrossN * invInertiaA +
							 rbCrossN * rbCrossN * invInertiaB;

			float j = -(1 + restitution) * velocityAlongNormal / invMassSum;
			MyVector impulse = normal * j;

			// 5. Применение импульсов
			if (!aIsStatic)
			{
				a.AddForce(impulse * a.InverseMass * -1);
				a.AddTorque(ra.Cross(impulse) * a.InverseInertia * -1);
			}

			if (!b.IsStatic)
			{
				b.AddForce(impulse * b.InverseMass);
				b.AddTorque(rb.Cross(impulse) * b.InverseInertia);
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
}