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
}