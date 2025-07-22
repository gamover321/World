using World.Engine.Primitives;

namespace World.Engine.My2dWorld.Primitives;

public class PhysicsStar : PhysicsBaseEntity
{
	public float Width { get; set; }
	public float Height { get; set; }

	public PhysicsStar(float x1, float y1, float width, float height, float mass) : base(x1, y1, mass)
	{
		Width = width;
		Height = height;

		Points = CreatePentagon(width, height);
		// формула для прямоугольников
		Inertia = mass * (width * width + height * height) / 12f;
	}

	public static MyVector[] CreatePentagon(float width, float height)
	{
		var points = new MyVector[5];
		double angleStep = 2 * Math.PI / 5; // 72 градуса
		double startAngle = -Math.PI / 2;   // вершина вверх

		for (int i = 0; i < 5; i++)
		{
			double angle = startAngle + i * angleStep;
			float x = (float)Math.Cos(angle);
			float y = (float)Math.Sin(angle);

			// Масштабируем и сохраняем
			points[i] = new MyVector(x * width / 2, y * height / 2);
		}

		return points;
	}
}