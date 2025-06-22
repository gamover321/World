using World.Engine.Primitives;

namespace World.Engine.My2dWorld
{
	public class Camera2d
	{
		public MyVector Position { get; set; } = MyVector.Empty(); // Центр камеры в мире
		public float Zoom { get; set; } = 1.0f;                     // Масштаб
		public float Rotation { get; set; } = 0.0f;                 // (опц.) Поворот камеры

		public MyVector WorldToScreen(MyVector worldPos, MyVector screenCenter)
		{
			var offset = worldPos - Position;
			var scaled = offset * Zoom;

			// без поворота:
			return scaled + screenCenter;
		}

		public MyVector[] WorldToScreen(MyVector[] worldPos, MyVector screenCenter)
		{
			var result = new MyVector[worldPos.Length];

			for (var i = 0; i < worldPos.Length; i++)
			{
				result[i] = WorldToScreen(worldPos[i], screenCenter);
			}

			return result;
		}

		public MyVector ScreenToWorld(MyVector screenPos, MyVector screenCenter)
		{
			var offset = screenPos - screenCenter;
			var unscaled = offset / Zoom;
			return unscaled + Position;
		}
	}
}

