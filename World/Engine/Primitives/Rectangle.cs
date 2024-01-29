using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace World.Engine.Vector
{
    public class Rectangle
    {
        public float X1 => V1.X1;
        public float Y1 => V1.Y1;

        public float X2 => V2.X1;
        public float Y2 => V2.Y1;

        public float X3 => V3.X1;
        public float Y3 => V3.Y1;

        public float X4 => V4.X1;
        public float Y4 => V4.Y1;

        public Rectangle(float x1, float y1, float width, float height)
        {
            V1 = new Vector.Vector(x1, y1, x1 + width, y1);
            V2 = new Vector.Vector(x1 + width, y1, x1 + width, y1 + height);
            V3 = new Vector.Vector(x1 + width, y1 + height, x1, y1 + height);
            V4 = new Vector.Vector(x1, y1 + height, x1, y1);
        }

        public void Rotate(double angleDeg, double x, double y)
        {
            V1.Rotate(angleDeg, x, y);
            V2.Rotate(angleDeg, x, y);
            V3.Rotate(angleDeg, x, y);
            V4.Rotate(angleDeg, x, y);
        }

        private Vector.Vector V1 { get; set; }
        private Vector.Vector V2 { get; set; }
        private Vector.Vector V3 { get; set; }
        private Vector.Vector V4 { get; set; }
    }
}
