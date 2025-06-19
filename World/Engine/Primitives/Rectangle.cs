namespace World.Engine.Primitives
{
    public class Rectangle
    {
        public float X1 => V1.X;
        public float Y1 => V1.Y;

        public float X2 => V2.X;
        public float Y2 => V2.Y;

        public float X3 => V3.X;
        public float Y3 => V3.Y;

        public float X4 => V4.X;
        public float Y4 => V4.Y;

        public Rectangle(float width, float height)
        {
            V1 = new MyVector(0, 0);
            V2 = new MyVector(width, 0);
            V3 = new MyVector(width, height);
            V4 = new MyVector(0, height);
        }

        //ToDo: вынести положение в пространстве в физический примитив, тут оставить только координаты относительно 0
        public void Move(MyVector myVector)
        {
            V1.Add(myVector);
            V2.Add(myVector);
            V3.Add(myVector);
            V4.Add(myVector);
        }

        public void Rotate(double angleDeg, double x, double y)
        {
            V1.Rotate(angleDeg, x, y);
            V2.Rotate(angleDeg, x, y);
            V3.Rotate(angleDeg, x, y);
            V4.Rotate(angleDeg, x, y);
        }

        private MyVector V1 { get; set; }
        private MyVector V2 { get; set; }
        private MyVector V3 { get; set; }
        private MyVector V4 { get; set; }
    }
}
