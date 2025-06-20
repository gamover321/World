namespace World.Engine.Primitives;

public class MyVector
{
    public MyVector(float x, float y)
    {
        X = x;
        Y = y;
    }

    public float X { get; set; } 
    public float Y { get; set; }

    // сложение
    public MyVector Add(MyVector another)
    {
	    return new MyVector(X + another.X, Y + another.Y);
    }

    // вычитание
    public MyVector Sub(MyVector another)
    {
	    return new MyVector(X - another.X, Y - another.Y);
    }

	public (double, double) GetMiddle()
    {
        var x = X / 2;
        var y = Y / 2;

        return (x, y);
    }

	public MyVector GetNormal()
	{
		return new MyVector(-Y, X).Normalize();
	}

	public MyVector Normalize()
    {
        var dist = Math.Sqrt(X * X + Y * Y);
        return Mul(1/(float)dist);
    }

    public MyVector Mul(float val)
    {
	    return new MyVector(X * val, Y*val);
    }

	// Скалярное произведение векторов
	public float Dot(MyVector other)
    {
	    return X * other.X + Y * other.Y;
    }

	public float DistanceTo(MyVector other)
    {
        var val = Math.Pow(other.X - X, 2) + Math.Pow(other.Y - Y, 2);
        return (float)Math.Sqrt(val);
    }

    public MyVector Copy()
    {
        return new MyVector(X, Y);
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

    public float Length()
    {
	    return (float)Math.Sqrt(LengthSquared());
    }

	public float LengthSquared()
    {
	    return X * X + Y * Y;
    }

    public MyVector Perpendicular() => new MyVector(-Y, X); // поворот на 90° против часовой
    public float Cross(MyVector other) => X * other.Y - Y * other.X;

	public static MyVector Empty()
    {
        return new MyVector(0, 0);
    }
	public static MyVector operator *(MyVector v, float scalar) => new(v.X * scalar, v.Y * scalar);
	public static MyVector operator /(MyVector v, float scalar) => new(v.X / scalar, v.Y / scalar);
	public static MyVector operator -(MyVector a, MyVector b) => new(a.X - b.X, a.Y - b.Y);
	public static MyVector operator +(MyVector a, MyVector b) => new(a.X + b.X, a.Y + b.Y);

	private double DegToRad(double deg)
    {
        var result = deg * Math.PI / 180;
        return result;
    }

	public override string ToString()
	{
		return $"{X};{Y}";
	}
}