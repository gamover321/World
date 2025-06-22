using SkiaSharp;
using World.Engine.My2dWorld;
using World.Engine.My2dWorld.Primitives;
using World.Engine.Primitives;

namespace World.Engine.Render.Primitives;

public interface IEntityRender
{
	void Render(SKCanvas canvas, Camera2d camera2d);
}

public class BaseEntityRender : IEntityRender
{
	private PhysicsBaseEntity entity { get; set; }

	public BaseEntityRender(PhysicsBaseEntity entity)
	{
		this.entity = entity;
	}

	public virtual void Render(SKCanvas canvas, Camera2d camera2d)
	{
		float? prevX = null;
		float? prevY = null;

		var transformedEdges = entity.GetTransformedPoints(PhysicsContext.Actual);

		var edgesToCamera = camera2d.WorldToScreen(transformedEdges, MyVector.Empty());

		for (var i = 0; i < edgesToCamera.Length; i++)
		{
			var edge = edgesToCamera[i];
			var nextEdge = edgesToCamera[(i + 1) % edgesToCamera.Length];

			canvas.DrawLine(edge.X, edge.Y, nextEdge.X, nextEdge.Y,
				new SKPaint { Color = SKColor.Parse("#000000") });
		}
	}
}