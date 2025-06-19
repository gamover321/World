using World.Engine.Primitives;

namespace World.Engine.My2dWorld.Collisions;

public class CollisionInfo
{
	public bool Intersects;
	public MyVector Normal; // направлена от A к B
	public float Penetration;
	public MyVector ContactPoint { get; set; } = new(0, 0);
}