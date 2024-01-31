using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace World.Engine.My2dWorld.Primitives
{
    public class PhysicsBaseEntity : IPhysicsEntity
    {
        #region Public properties

        public Vector.Vector Position { get; set; }

        public Vector.Vector Velocity { get; set; }
        public Vector.Vector Acceleration { get; set; }


        public float AngularPosition { get; set; }
        public float AngularVelocity { get; set; }
        public float AngularAcceleration { get; set; }


        public Dictionary<string, Vector.Vector> Forces { get; set; }

        public float Mass { get; set; }

        public Vector.Vector[] Edges { get; set; }

        #endregion


        public PhysicsBaseEntity(float x, float y, float mass)
        {
            Position = new Vector.Vector(x, y);

            AngularPosition = 0;

            Velocity = Vector.Vector.Empty();
            AngularVelocity = 0f;

            Acceleration = Vector.Vector.Empty();
            AngularAcceleration = 0f;

            Forces = new Dictionary<string, Vector.Vector>();
            Mass = mass;
        }

        public void Update()
        {
            Acceleration = GetAcceleration();

            Debug.WriteLine($"acc: {Acceleration.X}");

            Velocity.Add(Acceleration);
            AngularVelocity += AngularAcceleration;

            Rotate();

            Position.Add(Velocity);
            AngularPosition += AngularVelocity;
        }

        public void AddForce(string name, Vector.Vector vector)
        {
            Forces[name] = vector;
        }

        public void RemoveForce(string name)
        {
            if (Forces.ContainsKey(name))
            {
                Forces.Remove(name);
            }
        }

        public void SetSpeed(Vector.Vector vector)
        {
            Velocity = vector;
        }

        public void SetAngularSpeed(float speed)
        {
            AngularVelocity = speed;
        }

        //public void SetAcceleration(Vector.Vector vector)
        //{
        //    Acceleration = vector;
        //}

        public void SetAngularAcceleration(float speed)
        {
            AngularAcceleration = speed;
        }


        public void Rotate()
        {
            foreach (var edge in Edges)
            {
                edge.Rotate(AngularVelocity, 0, 0); // -> относительно вершин, а не текущего положения
            }
        }

        public bool IsIntersect(PhysicsBaseEntity other)
        {
            var thisRadius = GetRadius();
            var otherRadius = other.GetRadius();

            var length = Position.DistanceTo(other.Position);

            var result = length <= thisRadius+otherRadius;
            return result;
        }

        public float GetRadius()
        {
            var r = 0f;
            foreach (var edge in Edges)
            {
                var edgeLength = edge.DistanceTo(new Vector.Vector(0, 0));
                if (r < edgeLength)
                {
                    r = edgeLength;
                }
            }

            return r;
        }

        private Vector.Vector GetAcceleration()
        {
            var sum = Vector.Vector.Empty();
            foreach (var force in Forces)
            {
                sum.Add(force.Value);
            }

            sum.Mul(1/Mass);

            return sum;
        }
    }
}
