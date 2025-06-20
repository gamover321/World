using World.Engine.Primitives;

namespace World.Engine.My2dWorld.Primitives;

public class PhysicsRectangle : PhysicsBaseEntity
{
	public float Width { get; set; }
	public float Height { get; set; }

	public PhysicsRectangle(float x1, float y1, float width, float height, float mass) : base(x1, y1, mass)
	{
		Width = width;
		Height = height;

		Points = new[]
		{
			new MyVector(-width/2, -height/2),
			new MyVector(width/2, -height/2),
			new MyVector(width / 2, height/2),
			new MyVector(-width/2, height/2)
		};

		// формула для прямоугольников
		Inertia = mass * (width * width + height * height) / 12f;
	}

	public override bool IsIntersect(PhysicsBaseEntity other)
	{
		var axes = GetNormals().Concat(other.GetNormals());

		foreach (var axis in axes)
		{
			var (minA, maxA) = ProjectOntoAxis(axis);
			var (minB, maxB) = other.ProjectOntoAxis(axis);

			if (maxA < minB || maxB < minA)
			{
				// Существует разделяющая ось → нет пересечения
				return false;
			}
		}

		return true; // Все проекции пересеклись → есть пересечение
	}

	public override bool IsIntersectWithMTV(PhysicsBaseEntity other, out MyVector mtvNormal, out float penetrationDepth)
	{
		float minOverlap = float.MaxValue;
		MyVector smallestAxis = null;

		var axes = GetNormals().Concat(other.GetNormals());

		foreach (var axis in axes)
		{
			var (minA, maxA) = ProjectOntoAxis(axis);
			var (minB, maxB) = other.ProjectOntoAxis(axis);

			if (maxA < minB || maxB < minA)
			{
				// Разделяющая ось найдена - пересечения нет
				mtvNormal = null;
				penetrationDepth = 0;
				return false;
			}

			// Вычисляем перекрытие интервалов на этой оси
			float overlap = Math.Min(maxA, maxB) - Math.Max(minA, minB);
			if (overlap < minOverlap)
			{
				minOverlap = overlap;
				smallestAxis = axis;
			}
		}

		// MTV — вектор минимального перемещения для выхода из пересечения
		mtvNormal = smallestAxis;
		penetrationDepth = minOverlap;

		// Направление нормали должно смотреть от this к other
		var centerDir = other.Position.Copy();
		centerDir.Sub(Position);
		if (mtvNormal.ScalarMul(centerDir) < 0)
		{
			mtvNormal.Mul(-1);
		}

		return true;
	}

}