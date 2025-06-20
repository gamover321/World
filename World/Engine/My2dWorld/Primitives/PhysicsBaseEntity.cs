using System.Diagnostics;
using System.Windows.Forms.VisualStyles;
using World.Engine.My2dWorld.Collisions;
using World.Engine.Primitives;

namespace World.Engine.My2dWorld.Primitives;

public abstract class PhysicsBaseEntity : IPhysicsEntity
{
	#region Public properties

	public string Name { get; set; } = "Entity";

	#region Линейное движение
	public MyVector Position { get; set; }
	public MyVector Speed { get; set; }
	public MyVector Acceleration { get; set; }
	public float Mass { get; set; }
	public float InverseMass => Mass == 0 ? 0 : 1 / Mass;
	#endregion

	#region Вращательное движение
	public float AngularPosition { get; set; }
	public float AngularSpeed { get; set; }
	public float AngularAcceleration { get; set; }
	public float Inertia { get; protected set; }
	public float InverseInertia => Inertia == 0 ? 0 : 1 / Inertia;
	#endregion

	public Dictionary<string, MyVector> Forces { get; set; } = new();
	public Dictionary<string, float> Torques { get; set; } = new();

	public MyVector[] Points { get; init; }
	public List<MyVector> CollisionPoints { get; set; } = new();

	#endregion

	protected PhysicsBaseEntity(float x, float y, float mass)
	{
		Position = new MyVector(x, y);
		Speed = MyVector.Empty();

		AngularPosition = 0;
		AngularSpeed = 0f;

		Acceleration = MyVector.Empty();
		AngularAcceleration = 0f;

		Mass = mass;
	}

	public virtual void Update()
	{
		Acceleration = GetAcceleration();
		AngularAcceleration = GetAngularAcceleration();

		Speed = Speed.Add(Acceleration);
		AngularSpeed += AngularAcceleration;

		Position = Position.Add(Speed);
		AngularPosition += AngularSpeed;

		OnUpdate();

		Debug.WriteLine($"acc: {Acceleration.X}, ang.acc: {AngularAcceleration}");
	}

	public override string ToString()
	{
		return $"{Name}: {Position.X};{Position.Y}";
	}

	public virtual void OnUpdate() { }

	public void AddForce(string name, MyVector myVector) => Forces[name] = myVector;
	public void RemoveForce(string name) => Forces.Remove(name);

	public void AddTorque(string name, float torque) => Torques[name] = torque;
	public void RemoveTorque(string name) => Torques.Remove(name);

	public abstract bool IsIntersect(PhysicsBaseEntity other);
	public abstract bool IsIntersectWithMTV(PhysicsBaseEntity other, out MyVector mtvNormal, out float penetrationDepth);

	public float GetRadius()
	{
		var r = 0f;
		foreach (var edge in Points)
		{
			var edgeLength = edge.DistanceTo(MyVector.Empty());
			if (r < edgeLength)
				r = edgeLength;
		}
		return r;
	}

	public List<MyVector> GetNormals()
	{
		var normals = new List<MyVector>();
		var edges = GetTransformedEdges(); // edges — уже векторы рёбер

		for (var i = 0; i < edges.Length; i++)
		{
			var edge = edges[i];
			var normal = edge.GetNormal().Normalize();
			normals.Add(normal);
		}

		return normals;
	}

	public MyVector[] GetTransformedEdges()
	{
		var points = GetTransformedPoints();
		var edges = new MyVector[points.Length];

		for (int i = 0; i < points.Length; i++)
		{
			var nextIndex = (i + 1) % points.Length;
			edges[i] = points[nextIndex].Sub(points[i]);
		}

		return edges;
	}

	public MyVector[] GetTransformedPoints()
	{
		var transformed = new MyVector[Points.Length];

		var cos = (float)Math.Cos(AngularPosition);
		var sin = (float)Math.Sin(AngularPosition);

		for (var i = 0; i < Points.Length; i++)
		{
			var local = Points[i];

			var rotatedX = local.X * cos - local.Y * sin;
			var rotatedY = local.X * sin + local.Y * cos;

			transformed[i] = new MyVector(rotatedX + Position.X, rotatedY + Position.Y);
		}
		return transformed;
	}

	public MyVector GetAcceleration()
	{
		if (InverseMass == 0)
			return MyVector.Empty();

		var sum = MyVector.Empty();
		foreach (var force in Forces.Values)
		{
			sum = sum.Add(force);
		}

		sum.Mul(InverseMass);
		return sum;
	}

	public float GetAngularAcceleration()
	{
		if (InverseInertia == 0)
			return 0f;

		float totalTorque = 0f;
		foreach (var t in Torques.Values)
			totalTorque += t;

		return totalTorque * InverseInertia;
	}

	public (float min, float max) ProjectOntoAxis(MyVector axis)
	{
		var globalPoints = GetTransformedPoints();

		var min = axis.ScalarMul(globalPoints[0]);
		var max = min;

		for (int i = 1; i < globalPoints.Length; i++)
		{
			var proj = axis.ScalarMul(globalPoints[i]);
			if (proj < min) min = proj;
			if (proj > max) max = proj;
		}

		return (min, max);
	}

	public CollisionInfo GetCollisionInfo(PhysicsBaseEntity other)
	{
		var axes = GetNormals().Concat(other.GetNormals());

		var minPenetration = float.MaxValue;
		MyVector smallestAxis = null;

		foreach (var axis in axes)
		{
			var (minA, maxA) = ProjectOntoAxis(axis);
			var (minB, maxB) = other.ProjectOntoAxis(axis);

			float overlap = Math.Min(maxA, maxB) - Math.Max(minA, minB);
			if (overlap < 0)
			{
				return new CollisionInfo { Intersects = false };
			}

			if (overlap < minPenetration)
			{
				minPenetration = overlap;
				smallestAxis = axis; // <--- сохраняем нормализованную ось
			}
		}

		if (smallestAxis == null)
		{
			return new CollisionInfo { Intersects = false };
		}

		var direction = other.Position.Sub(Position);
		if (smallestAxis.ScalarMul(direction) < 0)
		{
			smallestAxis = new MyVector(-smallestAxis.X, -smallestAxis.Y);
		}

		var contactPoint = GetContactPoint(other, smallestAxis);

		return new CollisionInfo
		{
			Intersects = true,
			Normal = smallestAxis.Normalize(),           // <-- уже нормализована
			Penetration = minPenetration,
			ContactPoint = contactPoint
		};
	}

	private MyVector GetContactPoint(PhysicsBaseEntity other, MyVector normal)
	{
		normal = normal.Normalize();

		var pointsA = GetTransformedPoints();
		var pointsB = other.GetTransformedPoints();

		// Касательный вектор — перпендикуляр к нормали
		var tangent = new MyVector(normal.Y, -normal.X);

		const float epsilon = 0.01f;

		// Проекция на нормаль, чтобы выбрать крайние точки
		float minProjA = float.MaxValue;
		List<MyVector> candidatesA = new();

		foreach (var v in pointsA)
		{
			float proj = v.ScalarMul(normal);
			if (proj < minProjA - epsilon)
			{
				minProjA = proj;
				candidatesA.Clear();
				candidatesA.Add(v);
			}
			else if (Math.Abs(proj - minProjA) <= epsilon)
			{
				candidatesA.Add(v);
			}
		}

		// Среди кандидатов выбираем по касательной
		MyVector bestA = candidatesA[0];
		float bestTangentProjA = bestA.ScalarMul(tangent);

		foreach (var v in candidatesA)
		{
			float tangentProj = v.ScalarMul(tangent);
			// Выбираем точку с минимальной (или максимальной — зависит от направления) касательной проекцией
			if (tangentProj < bestTangentProjA)
			{
				bestTangentProjA = tangentProj;
				bestA = v;
			}
		}

		// Аналогично для B (берём максимальную проекцию по нормали)
		float maxProjB = float.MinValue;
		List<MyVector> candidatesB = new();

		foreach (var v in pointsB)
		{
			float proj = v.ScalarMul(normal);
			if (proj > maxProjB + epsilon)
			{
				maxProjB = proj;
				candidatesB.Clear();
				candidatesB.Add(v);
			}
			else if (Math.Abs(proj - maxProjB) <= epsilon)
			{
				candidatesB.Add(v);
			}
		}

		MyVector bestB = candidatesB[0];
		float bestTangentProjB = bestB.ScalarMul(tangent);

		foreach (var v in candidatesB)
		{
			float tangentProj = v.ScalarMul(tangent);
			if (tangentProj > bestTangentProjB) // обычно для другой стороны выбираем max
			{
				bestTangentProjB = tangentProj;
				bestB = v;
			}
		}

		// Средняя точка контакта
		return new MyVector(
			(bestA.X + bestB.X) / 2f,
			(bestA.Y + bestB.Y) / 2f
		);
	}




	private MyVector AverageVector(List<MyVector> vectors)
	{
		float x = 0, y = 0;
		foreach (var v in vectors)
		{
			x += v.X;
			y += v.Y;
		}
		int count = vectors.Count;
		return new MyVector(x / count, y / count);
	}

	public static CollisionInfo GetCollisionInfo(PhysicsBaseEntity a, PhysicsBaseEntity b)
	{
		var axes = a.GetNormals().Concat(b.GetNormals());

		float minPenetration = float.MaxValue;
		MyVector smallestAxis = default;

		foreach (var axis in axes)
		{
			var normAxis = axis.Normalize();

			var (minA, maxA) = a.ProjectOntoAxis(normAxis);
			var (minB, maxB) = b.ProjectOntoAxis(normAxis);

			float overlap = Math.Min(maxA, maxB) - Math.Max(minA, minB);
			if (overlap < 0)
			{
				return new CollisionInfo { Intersects = false };
			}

			if (overlap < minPenetration)
			{
				minPenetration = overlap;
				smallestAxis = normAxis;
			}
		}

		if (smallestAxis == null)
			return new CollisionInfo { Intersects = false };

		var direction = b.Position - a.Position;
		if (smallestAxis.ScalarMul(direction) < 0)
		{
			smallestAxis = smallestAxis*-1;
		}

		var contactPoint = GetContactPoint(a, b, smallestAxis);

		return new CollisionInfo
		{
			Intersects = true,
			Normal = smallestAxis,
			Penetration = minPenetration,
			ContactPoint = contactPoint
		};
	}

	public static MyVector GetContactPoint(PhysicsBaseEntity a, PhysicsBaseEntity b, MyVector normal)
	{
		var aVertices = a.GetTransformedPoints();
		var bVertices = b.GetTransformedPoints();

		// Найдём вершину объекта A, которая ближе всех вдоль нормали к B
		MyVector bestA = aVertices[0];
		float minA = bestA.ScalarMul(normal);

		foreach (var v in aVertices)
		{
			float proj = v.ScalarMul(normal);
			if (proj < minA)
			{
				minA = proj;
				bestA = v;
			}
		}

		// Найдём вершину объекта B, которая ближе всех вдоль обратной нормали к A
		MyVector bestB = bVertices[0];
		float maxB = bestB.ScalarMul(normal);

		foreach (var v in bVertices)
		{
			float proj = v.ScalarMul(normal);
			if (proj > maxB)
			{
				maxB = proj;
				bestB = v;
			}
		}

		// Вернём среднюю точку — компромисс
		return new MyVector(
			(bestA.X + bestB.X) / 2f,
			(bestA.Y + bestB.Y) / 2f
		);
	}


}
