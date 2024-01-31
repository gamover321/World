namespace World.Engine.My2dWorld.Primitives;

public interface IPhysicsEntity
{
    Vector.Vector Position { get; set; }
    Vector.Vector Velocity { get; set; }
    Vector.Vector Acceleration { get; set; }
    float AngularPosition { get; set; }
    float AngularVelocity { get; set; }
    float AngularAcceleration { get; set; }
    Vector.Vector[] Edges { get; set; }
    void Update();
    void SetSpeed(Vector.Vector vector);
    void SetAngularSpeed(float speed);
    void SetAngularAcceleration(float speed);
    void Rotate();

    void AddForce(string name, Vector.Vector vector);
    void RemoveForce(string name);
}