using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace World.Engine.Vector;

public class Vector
{
    public Vector(float x, float y)
    {
        X = x;
        Y = y;
    }

    public float X { get; private set; } 
    public float Y { get; private set; }

    public void Add(Vector another)
    {
        X += another.X;
        Y += another.Y;
    }

    public (double, double) GetMiddle()
    {
        var x = X / 2;
        var y = Y / 2;

        return (x, y);
    }

    public void Normalize()
    {
        var dist = Math.Sqrt(X * X + Y * Y);
        Mul(1/(float)dist);
    }

    public void Mul(float val)
    {
        X *= val;
        Y *= val;
    }

    public float DistanceTo(Vector other)
    {
        var val = Math.Pow(other.X - X, 2) + Math.Pow(other.Y - Y, 2);
        return (float)Math.Sqrt(val);
    }

    public Vector Copy()
    {
        return new Vector(X, Y);
    }

    public void Rotate(double angleDeg, double x, double y)
    {
        var angleRad = DegToRad(angleDeg);

        var (sin, cos) = Math.SinCos(angleRad);

        var prevX = X;
        var prevY = Y;

        X = (float)((prevX-x) * cos - (prevY-y) * sin +x);
        Y = (float)((prevX-x) * sin + (prevY-y) * cos + y);
    }

    public static Vector Empty()
    {
        return new Vector(0, 0);
    }

    private double DegToRad(double deg)
    {
        var result = deg * Math.PI / 180;
        return result;
    }

}