using World.Engine.Primitives;

namespace World.Engine.My2dWorld.Primitives;

public interface IPhysicsEntity
{
	MyVector Position { get; set; }
	MyVector Velocity { get; set; }
	MyVector Acceleration { get; set; }
	float Mass { get; set; }
	float InverseMass { get; }
	float AngularPosition { get; set; }
	float AngularVelocity { get; set; }
	float AngularAcceleration { get; set; }
	float Inertia { get; }
	float InverseInertia { get; }
	Dictionary<string, MyVector> Forces { get; set; }
	Dictionary<string, float> Torques { get; set; }
	MyVector[] Points { get; init; }
	void Update();
	void OnUpdate();
	void AddForce(MyVector myVector, string name);
	void RemoveForce(string name);
	void AddTorque(float torque, string name);
	void RemoveTorque(string name);
	bool IsIntersect(PhysicsBaseEntity other);
	float GetRadius();
	List<MyVector> GetNormals();
	MyVector[] GetTransformedPoints();
	MyVector GetAcceleration();
	float GetAngularAcceleration();
	(float min, float max) ProjectOntoAxis(MyVector axis);
}