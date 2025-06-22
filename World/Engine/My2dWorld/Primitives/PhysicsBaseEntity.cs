using System.Diagnostics;
using System.Numerics;
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
	public MyVector PredictedPosition { get; set; }
	public MyVector Velocity { get; set; }
	public MyVector Acceleration { get; set; }
	public float Mass { get; set; }
	public float InverseMass => Mass == 0 ? 0 : 1 / Mass;
	public bool IsStatic => InverseMass == 0;

	#endregion

	#region Вращательное движение
	public float Angle { get; set; }
	public float PredictedAngle { get; set; }
	public float AngularVelocity { get; set; }
	public float AngularAcceleration { get; set; }
	public float Inertia { get; protected set; } = 1;
	public float InverseInertia => Inertia == 0 ? 0 : 1 / Inertia;
	#endregion

	public Dictionary<string, MyVector> Forces { get; init; } = new();
	public Dictionary<string, float> Torques { get; init; } = new();

	public MyVector[] Points { get; init; }
	public List<MyVector> CollisionPoints { get; set; } = new();

	#endregion

	public MyVector GetPosition(PhysicsContext ctx) => ctx.UsePredicted ? PredictedPosition : Position;
	public float GetAngle(PhysicsContext ctx) => ctx.UsePredicted ? PredictedAngle : Angle;

	protected PhysicsBaseEntity(float x, float y, float mass)
	{
		Position = new MyVector(x, y);
		PredictedPosition = Position;

		Velocity = MyVector.Empty();

		Angle = 0;
		AngularVelocity = 0f;

		Acceleration = MyVector.Empty();
		AngularAcceleration = 0f;

		Mass = mass;
	}

	public override string ToString()
	{
		return $"{Name}: {Position.X};{Position.Y}";
	}

	public virtual void OnUpdate() { }

	public void AddForce(MyVector myVector, string name = "")
	{
		if (string.IsNullOrEmpty(name))
		{
			name = Guid.NewGuid().ToString();
		}
		Forces[name] = new MyVector(myVector.X, myVector.Y);
	}
	public void RemoveForce(string name) => Forces.Remove(name);

	public void ClearAllForces() => Forces.Clear();

	
	public void AddTorque(float torque, string name ="")
	{
		if (string.IsNullOrEmpty(name))
		{
			name = Guid.NewGuid().ToString();
		}
		Torques[name] = torque;
	}
	public void RemoveTorque(string name) => Torques.Remove(name);
	public void ClearTorque() => Torques.Clear();

	public MyVector GetTotalAcceleration()
	{
		if (IsStatic)
		{
			return MyVector.Empty();
		}

		var sum = MyVector.Empty();
		foreach (var force in Forces.Values)
		{
			sum = sum.Add(force);
		}

		sum *= InverseMass;
		return sum;
	}

	public float GetTotalTorque()
	{
		if (IsStatic)
		{
			return 0f;
		}

		float totalTorque = 0f;
		foreach (var t in Torques.Values)
		{
			totalTorque += t;
		}

		//return totalTorque * InverseInertia;
		return totalTorque;
	}

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

	public List<MyVector> GetNormals(PhysicsContext ctx)
	{
		var normals = new List<MyVector>();
		var edges = GetTransformedEdges(ctx); // edges — уже векторы рёбер

		for (var i = 0; i < edges.Length; i++)
		{
			var edge = edges[i];
			var normal = edge.GetNormal().Normalize();
			normals.Add(normal);
		}

		return normals;
	}

	public MyVector[] GetTransformedEdges(PhysicsContext ctx)
	{
		var points = GetTransformedPoints(ctx);
		var edges = new MyVector[points.Length];

		for (int i = 0; i < points.Length; i++)
		{
			var nextIndex = (i + 1) % points.Length;
			edges[i] = points[nextIndex].Sub(points[i]);
		}

		return edges;
	}

	public MyVector[] GetTransformedPoints(PhysicsContext ctx)
	{
		var position = GetPosition(ctx);

		var transformed = new MyVector[Points.Length];

		var cos = (float)Math.Cos(Angle);
		var sin = (float)Math.Sin(Angle);

		for (var i = 0; i < Points.Length; i++)
		{
			var local = Points[i];

			var rotatedX = local.X * cos - local.Y * sin;
			var rotatedY = local.X * sin + local.Y * cos;

			transformed[i] = new MyVector(rotatedX + position.X, rotatedY + position.Y);
		}
		return transformed;
	}

	public (float min, float max) ProjectOntoAxis(MyVector axis, PhysicsContext ctx)
	{
		var globalPoints = GetTransformedPoints(ctx);

		var min = axis.Dot(globalPoints[0]);
		var max = min;

		for (int i = 1; i < globalPoints.Length; i++)
		{
			var proj = axis.Dot(globalPoints[i]);
			if (proj < min) min = proj;
			if (proj > max) max = proj;
		}

		return (min, max);
	}

	public CollisionInfo GetCollisionInfo(PhysicsBaseEntity other, PhysicsContext ctx)
	{
		var radius = GetRadius();
		var otherRadius = other.GetRadius();

		var position = GetPosition(ctx);
		var otherPosition = other.GetPosition(ctx);

		if ((position - otherPosition).LengthSquared() > Math.Pow(radius+otherRadius, 2))
		{
			return new CollisionInfo { Intersects = false };
		}

		var axes = GetNormals(ctx).Concat(other.GetNormals(ctx));
		
		var minPenetration = float.MaxValue;
		MyVector smallestAxis = null;

		foreach (var axis in axes)
		{
			var (minA, maxA) = ProjectOntoAxis(axis, ctx);
			var (minB, maxB) = other.ProjectOntoAxis(axis, ctx);

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

		var direction = otherPosition.Sub(position);
		if (smallestAxis.Dot(direction) < 0)
		{
			smallestAxis = new MyVector(-smallestAxis.X, -smallestAxis.Y);
		}

		var contactPoint = GetContactPoint(other, smallestAxis, ctx);

		return new CollisionInfo
		{
			Intersects = true,
			Normal = smallestAxis.Normalize(),           // <-- уже нормализована
			Penetration = minPenetration,
			ContactPoint = contactPoint
		};
	}

	private MyVector GetContactPoint(PhysicsBaseEntity other, MyVector normal, PhysicsContext ctx)
	{
		var pointsA = GetTransformedPoints(ctx);
		var pointsB = other.GetTransformedPoints(ctx);

		// Находим все точки, участвующие в столкновении
		List<MyVector> contactPoints = new List<MyVector>();

		// Проверяем вершины A против ребер B
		foreach (var point in pointsA)
		{
			if (IsPointInsidePolygon(point, pointsB))
			{
				contactPoints.Add(point);
			}
		}

		// Проверяем вершины B против ребер A
		foreach (var point in pointsB)
		{
			if (IsPointInsidePolygon(point, pointsA))
			{
				contactPoints.Add(point);
			}
		}

		// Если нашли точки контакта - возвращаем среднюю
		if (contactPoints.Count > 0)
		{
			MyVector sum = contactPoints[0];
			for (int i = 1; i < contactPoints.Count; i++)
			{
				sum = sum.Add(contactPoints[i]);
			}
			return sum*(1f/contactPoints.Count);
		}

		// Если нет вершин внутри (столкновение ребро-ребро)
		return FindEdgeEdgeContact(pointsA, pointsB, normal);
	}

	private MyVector FindEdgeEdgeContact(MyVector[] polyA, MyVector[] polyB, MyVector normal)
	{
		// Находим ближайшие ребра между полигонами
		(MyVector edgeAStart, MyVector edgeAEnd) = FindClosestEdge(polyA, polyB, normal);
		(MyVector edgeBStart, MyVector edgeBEnd) = FindClosestEdge(polyB, polyA, new MyVector(-normal.X, -normal.Y));

		// Находим точку пересечения ребер
		if (LineIntersection(edgeAStart, edgeAEnd, edgeBStart, edgeBEnd, out MyVector intersection))
		{
			return intersection;
		}

		// Если пересечения нет (параллельные ребра), возвращаем середину между ближайшими точками
		return new MyVector(
			(edgeAStart.X + edgeBStart.X) * 0.5f,
			(edgeAStart.Y + edgeBStart.Y) * 0.5f
		);
	}

	private (MyVector start, MyVector end) FindClosestEdge(MyVector[] polygon, MyVector[] otherPoly, MyVector normal)
	{
		float minDistance = float.MaxValue;
		int edgeIndex = 0;

		for (int i = 0; i < polygon.Length; i++)
		{
			int j = (i + 1) % polygon.Length;
			float distance = 0;

			foreach (var point in otherPoly)
			{
				distance += DistanceToEdge(point, polygon[i], polygon[j], normal);
			}

			if (distance < minDistance)
			{
				minDistance = distance;
				edgeIndex = i;
			}
		}

		int nextIndex = (edgeIndex + 1) % polygon.Length;
		return (polygon[edgeIndex], polygon[nextIndex]);
	}

	private float DistanceToEdge(MyVector point, MyVector edgeStart, MyVector edgeEnd, MyVector normal)
	{
		MyVector edge = edgeEnd.Sub(edgeStart);
		MyVector toPoint = point.Sub(edgeStart);

		float t = Math.Clamp(toPoint.Dot(edge) / edge.Dot(edge), 0, 1);
		MyVector projection = edgeStart.Add(edge.Mul(t));

		return point.Sub(projection).LengthSquared();
	}

	private bool LineIntersection(MyVector a1, MyVector a2, MyVector b1, MyVector b2, out MyVector intersection)
	{
		intersection = new MyVector(0, 0);

		MyVector r = a2.Sub(a1);
		MyVector s = b2.Sub(b1);
		float rxs = r.Cross(s);
		float qpxr = (b1.Sub(a1)).Cross(r);

		// Параллельные линии
		if (Math.Abs(rxs) < float.Epsilon) return false;

		float t = (b1.Sub(a1)).Cross(s) / rxs;
		float u = (b1.Sub(a1)).Cross(r) / rxs;

		if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
		{
			intersection = a1.Add(r.Mul(t));
			return true;
		}

		return false;
	}

	private bool IsPointInsidePolygon(MyVector point, MyVector[] polygon)
	{
		bool inside = false;
		for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
		{
			if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) &&
				(point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) /
						  (polygon[j].Y - polygon[i].Y) + polygon[i].X))
			{
				inside = !inside;
			}
		}
		return inside;
	}
}