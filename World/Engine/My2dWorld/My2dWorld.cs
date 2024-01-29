using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace World.Engine.My2dWorld
{
    public class My2dWorld
    {
        public int Width { get; }
        public int Height { get; }

        public Vector.Rectangle Position { get; set; }
        private Vector.Vector Velocity { get; set; }

        private double Angle { get; set; }


        private Vector.Vector Gravity { get; set; }
        private Vector.Vector CustomForce { get; set; }

        public My2dWorld(int width, int height)
        {
            Width = width;
            Height = height;

            Position = new Primitives.Rectangle(300f, 300f, 100f, 100f);
            //Velocity = new Vector.Vector(3, 0);

            //Gravity = new Vector.Vector(0, 0.5f);

            //CustomForce = new Vector.Vector(0, 0.0f);

            Angle = 0;
        }

        public void AddCustomForce(Vector.Vector f)
        {
            CustomForce = f;z
        }

        public void Update()
        {
            //var middle = Position.GetMiddle();

            Position.Rotate(1, Position.X1, Position.Y1);

            /*Velocity.Add(Gravity);
            Velocity.Add(CustomForce);
            Position.Add(Velocity);

            if (Position.X < 0)
            {
                Position.X = 0;
            }

            if (Position.X > Width)
            {
                Position.X = Width;
            }

            if (Position.X <= 0 || Position.X >= Width)
            {
                Velocity.X *= -0.95f;
            }

            if (Position.Y < 0)
            {
                Position.Y = 0;
            }

            if (Position.Y > Height)
            {
                Position.Y = Height;
            }
            if (Position.Y <= 0 || Position.Y >= Height)
            {
                Velocity.Y *= (float)-0.9;

                if (Velocity.X > 0)
                {
                    Velocity.X -= Math.Min(Velocity.X, 0.01f);
                }
            }*/
        }
    }
}
