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
	public MyVector Velocity { get; set; }
	public MyVector Acceleration { get; set; }
	public float Mass { get; set; }
	public float InverseMass => Mass == 0 ? 0 : 1 / Mass;
	public bool IsStatic => InverseMass == 0;

	#endregion

	#region Вращательное движение
	public float AngularPosition { get; set; }
	public float AngularVelocity { get; set; }
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
		Velocity = MyVector.Empty();

		AngularPosition = 0;
		AngularVelocity = 0f;

		Acceleration = MyVector.Empty();
		AngularAcceleration = 0f;

		Mass = mass;
	}

	public virtual void Update()
	{
		Acceleration = GetAcceleration();
		AngularAcceleration = GetAngularAcceleration();

		Velocity = Velocity.Add(Acceleration);
		AngularVelocity += AngularAcceleration;

		Position = Position.Add(Velocity);
		AngularPosition += AngularVelocity;

		OnUpdate();

		Debug.WriteLine($"acc: {Acceleration.X}, ang.acc: {AngularAcceleration}");
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

	public void AddTorque(float torque, string name ="")
	{
		if (string.IsNullOrEmpty(name))
		{
			name = Guid.NewGuid().ToString();
		}
		Torques[name] = torque;
	}
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
		if (smallestAxis.Dot(direction) < 0)
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
		var pointsA = GetTransformedPoints();
		var pointsB = other.GetTransformedPoints();

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

	private (MyVector, MyVector) FindClosestPoints(List<MyVector> polyA, List<MyVector> polyB, MyVector normal)
	{
		MyVector closestA = polyA[0];
		MyVector closestB = polyB[0];
		float minDist = float.MaxValue;

		foreach (var pA in polyA)
		{
			foreach (var pB in polyB)
			{
				float dist = (pA.X - pB.X) * normal.X + (pA.Y - pB.Y) * normal.Y;
				if (dist < minDist)
				{
					minDist = dist;
					closestA = pA;
					closestB = pB;
				}
			}
		}

		return (closestA, closestB);
	}

	private bool IsVertexContact(List<MyVector> polygon, MyVector point)
	{
		return polygon.Any(p => Math.Abs(p.X - point.X) < 0.001f && Math.Abs(p.Y - point.Y) < 0.001f);
	}

	private MyVector FindEdgeContactPoint(List<MyVector> edgePoly, MyVector vertex, MyVector normal)
	{
		float minDist = float.MaxValue;
		MyVector contact = vertex;

		for (int i = 0; i < edgePoly.Count; i++)
		{
			int j = (i + 1) % edgePoly.Count;
			MyVector edgeStart = edgePoly[i];
			MyVector edgeEnd = edgePoly[j];

			// Проекция вершины на ребро
			MyVector edge = new MyVector(edgeEnd.X - edgeStart.X, edgeEnd.Y - edgeStart.Y);
			MyVector toVertex = new MyVector(vertex.X - edgeStart.X, vertex.Y - edgeStart.Y);

			float edgeLengthSq = edge.X * edge.X + edge.Y * edge.Y;
			float t = Math.Clamp((toVertex.X * edge.X + toVertex.Y * edge.Y) / edgeLengthSq, 0, 1);

			MyVector projection = new MyVector(
				edgeStart.X + t * edge.X,
				edgeStart.Y + t * edge.Y
			);

			float dist = (vertex.X - projection.X) * normal.X + (vertex.Y - projection.Y) * normal.Y;
			if (dist < minDist)
			{
				minDist = dist;
				contact = projection;
			}
		}

		return contact;
	}

	private List<MyVector> ClipEdges(List<MyVector> polyA, List<MyVector> polyB, MyVector normal)
	{
		var contacts = new List<MyVector>();

		// Находим ребро с максимальным перекрытием
		float maxOverlap = float.MinValue;
		int edgeIndex = 0;

		for (int i = 0; i < polyA.Count; i++)
		{
			int j = (i + 1) % polyA.Count;
			MyVector edge = new MyVector(polyA[j].X - polyA[i].X, polyA[j].Y - polyA[i].Y);
			float edgeLength = (float)Math.Sqrt(edge.X * edge.X + edge.Y * edge.Y);
			MyVector edgeNormal = new MyVector(-edge.Y / edgeLength, edge.X / edgeLength);

			float overlap = normal.X * edgeNormal.X + normal.Y * edgeNormal.Y;
			if (overlap > maxOverlap)
			{
				maxOverlap = overlap;
				edgeIndex = i;
			}
		}

		// Получаем ребро для клиппинга
		MyVector v1 = polyA[edgeIndex];
		MyVector v2 = polyA[(edgeIndex + 1) % polyA.Count];
		MyVector edgeDir = new MyVector(v2.X - v1.X, v2.Y - v1.Y);
		float edgeDirLength = (float)Math.Sqrt(edgeDir.X * edgeDir.X + edgeDir.Y * edgeDir.Y);
		MyVector edgeNormalized = new MyVector(edgeDir.X / edgeDirLength, edgeDir.Y / edgeDirLength);

		// Клиппим полигон B по этому ребру
		var clipped = ClipPolygon(polyB, v1, edgeNormalized);

		// Добавляем точки, которые находятся внутри полигона A
		foreach (var p in clipped)
		{
			if (IsInsidePolygon(polyA, p))
			{
				contacts.Add(p);
			}
		}

		return contacts;
	}

	private bool IsInsidePolygon(List<MyVector> polygon, MyVector point)
	{
		bool inside = false;
		for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
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

	private List<MyVector> ClipPolygon(List<MyVector> polygon, MyVector point, MyVector normal)
	{
		var result = new List<MyVector>();

		for (int i = 0; i < polygon.Count; i++)
		{
			int j = (i + 1) % polygon.Count;
			MyVector vi = polygon[i];
			MyVector vj = polygon[j];

			float di = (vi.X - point.X) * normal.X + (vi.Y - point.Y) * normal.Y;
			float dj = (vj.X - point.X) * normal.X + (vj.Y - point.Y) * normal.Y;

			if (di <= 0) result.Add(vi);
			if (di * dj < 0)
			{
				float t = Math.Abs(di) / (Math.Abs(di) + Math.Abs(dj));
				float intersectX = vi.X + (vj.X - vi.X) * t;
				float intersectY = vi.Y + (vj.Y - vi.Y) * t;
				result.Add(new MyVector(intersectX, intersectY));
			}
		}

		return result;
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
		if (smallestAxis.Dot(direction) < 0)
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
		float minA = bestA.Dot(normal);

		foreach (var v in aVertices)
		{
			float proj = v.Dot(normal);
			if (proj < minA)
			{
				minA = proj;
				bestA = v;
			}
		}

		// Найдём вершину объекта B, которая ближе всех вдоль обратной нормали к A
		MyVector bestB = bVertices[0];
		float maxB = bestB.Dot(normal);

		foreach (var v in bVertices)
		{
			float proj = v.Dot(normal);
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
