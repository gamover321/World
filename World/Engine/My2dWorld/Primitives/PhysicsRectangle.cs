using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rectangle = World.Engine.Primitives.Rectangle;

namespace World.Engine.My2dWorld.Primitives
{
    public class PhysicsRectangle : PhysicsBaseEntity
    {
        public float Width { get; set; }
        public float Height { get; set; }

        public PhysicsRectangle(float x1, float y1, float width, float height, float mass) : base(x1, y1, mass)
        {
            Width = width;
            Height = height;

            Edges = new[]
            {
                new Vector.Vector(-width/2, -height/2),
                new Vector.Vector(width/2, -height/2),
                new Vector.Vector(width / 2, height/2),
                new Vector.Vector(-width/2, height/2)
            };
            
        }
        
    }
}
