using World.Engine.Primitives;

namespace World.Engine.My2dWorld.Primitives;

public interface IPhysicsEntity
{
	MyVector Position { get; set; }
	MyVector Velocity { get; set; }
	MyVector Acceleration { get; set; }
	float Mass { get; set; }
	float InverseMass { get; }
	float Angle { get; set; }
	float AngularVelocity { get; set; }
	float AngularAcceleration { get; set; }
	float Inertia { get; }
	float InverseInertia { get; }
	Dictionary<string, MyVector> Forces { get; init; }
	Dictionary<string, float> Torques { get; init; }
	MyVector[] Points { get; init; }
	void OnUpdate();
	void AddForce(MyVector myVector, string name);
	void RemoveForce(string name);
	void AddTorque(float torque, string name);
	void RemoveTorque(string name);
	//bool IsIntersect(PhysicsBaseEntity other, PhysicsContext ctx);
	float GetRadius();
	List<MyVector> GetNormals(PhysicsContext ctx);
	MyVector[] GetTransformedPoints(PhysicsContext ctx);
	MyVector GetTotalAcceleration();
	float GetTotalTorque();
	(float min, float max) ProjectOntoAxis(MyVector axis, PhysicsContext ctx);
}