using System.Diagnostics;
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

	public MyVector[] Edges { get; init; }
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
		foreach (var edge in Edges)
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
		var edges = GetTransformedEdges();

		for (int i = 0; i < edges.Length; i++)
		{
			var p1 = edges[i];
			var p2 = edges[(i + 1) % edges.Length];

			var edge = p2.Sub(p1);

			var normal = new MyVector(-edge.Y, edge.X);
			normal.Normalize();

			normals.Add(normal);
		}
		return normals;
	}

	public MyVector[] GetTransformedEdges()
	{
		var transformed = new MyVector[Edges.Length];

		var cos = (float)Math.Cos(AngularPosition);
		var sin = (float)Math.Sin(AngularPosition);

		for (var i = 0; i < Edges.Length; i++)
		{
			var local = Edges[i];

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
		var edges = GetTransformedEdges();

		var min = axis.ScalarMul(edges[0]);
		var max = min;

		for (int i = 1; i < edges.Length; i++)
		{
			var proj = axis.ScalarMul(edges[i]);
			if (proj < min) min = proj;
			if (proj > max) max = proj;
		}

		return (min, max);
	}

	public CollisionInfo GetCollisionInfo(PhysicsBaseEntity other)
	{
		var axes = GetNormals().Concat(other.GetNormals());

		float minPenetration = float.MaxValue;
		MyVector smallestAxis = null;

		foreach (var axis in axes)
		{
			var normAxis = axis; // <--- нормализуем до проекции ?? убрал

			var (minA, maxA) = ProjectOntoAxis(normAxis);
			var (minB, maxB) = other.ProjectOntoAxis(normAxis);

			float overlap = Math.Min(maxA, maxB) - Math.Max(minA, minB);
			if (overlap < 0)
			{
				return new CollisionInfo { Intersects = false };
			}

			if (overlap < minPenetration)
			{
				minPenetration = overlap;
				smallestAxis = normAxis; // <--- сохраняем нормализованную ось
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
		var edgesA = GetTransformedEdges();
		var edgesB = other.GetTransformedEdges();

		const float epsilon = 0.01f; // чуть больше для стабильности

		float minProjA = float.MaxValue;
		List<MyVector> contactCandidatesA = new();

		foreach (var v in edgesA)
		{
			float proj = v.ScalarMul(normal);
			if (proj < minProjA - epsilon)
			{
				minProjA = proj;
				contactCandidatesA.Clear();
				contactCandidatesA.Add(v);
			}
			else if (Math.Abs(proj - minProjA) <= epsilon)
			{
				contactCandidatesA.Add(v);
			}
		}

		float maxProjB = float.MinValue;
		List<MyVector> contactCandidatesB = new();

		foreach (var v in edgesB)
		{
			float proj = v.ScalarMul(normal);
			if (proj > maxProjB + epsilon)
			{
				maxProjB = proj;
				contactCandidatesB.Clear();
				contactCandidatesB.Add(v);
			}
			else if (Math.Abs(proj - maxProjB) <= epsilon)
			{
				contactCandidatesB.Add(v);
			}
		}

		var avgA = AverageVector(contactCandidatesA);
		var avgB = AverageVector(contactCandidatesB);

		return new MyVector((avgA.X + avgB.X) / 2, (avgA.Y + avgB.Y) / 2);
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
}
