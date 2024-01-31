using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace World.Engine.Vector;

public class Vector2
{
    public Vector2(float x1, float y1, float x2, float y2)
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
    }

    public Vector2(float x, float y)
    {
        X1 = 0;
        Y1 = 0;

        X2 = x;
        Y2 = y;
    }

    public float X1 { get; private set; } 
    public float Y1 { get; private set; }

    public float X2 { get; private set; }
    public float Y2 { get; private set; }

    public void Add(Vector2 another)
    {
        X1 += another.X1;
        Y1 += another.Y1;
        X2 += another.X2;
        Y2 += another.Y2;
    }

    public (double, double) GetMiddle()
    {
        var x = (X2 - X1) / 2 + X1;
        var y = (Y2 - Y1) / 2 + Y1;

        return (x, y);
    }

    public void Rotate(double angleDeg, double x, double y)
    {
        var angleRad = DegToRad(angleDeg);

        var (sin, cos) = Math.SinCos(angleRad);

        X1 = (float)((X1 - x) * cos - (Y1 - x) * sin + x);
        Y1 = (float)((X1 - y) * sin + (Y1 - y) * cos + y);

        X2 = (float)((X2-x) * cos - (Y2-x) * sin + x);
        Y2 = (float)((X2-y) * sin + (Y2-y) * cos + y);
    }

    public static Vector2 Empty()
    {
        return new Vector2(0, 0);
    }


    private double DegToRad(double deg)
    {
        var result = deg * Math.PI / 180;
        return result;
    }

}