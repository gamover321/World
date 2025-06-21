using SkiaSharp;
using World.Engine.My2dWorld;
using World.Engine.My2dWorld.Primitives;

namespace World.Engine.Render.Primitives;

public interface IEntityRender
{
	void Render(SKCanvas canvas);
}

public class BaseEntityRender : IEntityRender
{
	private PhysicsBaseEntity entity { get; set; }

	public BaseEntityRender(PhysicsBaseEntity entity)
	{
		this.entity = entity;
	}

	public virtual void Render(SKCanvas canvas)
	{
		float? prevX = null;
		float? prevY = null;

		var transformedEdges = entity.GetTransformedPoints(PhysicsContext.Actual);
		
		foreach (var edge in transformedEdges)
		{
			if (prevX == null || prevY == null)
			{
				prevX = edge.X;
				prevY = edge.Y;
				continue;
			}

			canvas.DrawLine(prevX.Value, prevY.Value, edge.X, edge.Y,
				new SKPaint { Color = SKColor.Parse("#000000") });

			prevX = edge.X;
			prevY = edge.Y;
		}

		// Замкнуть последнюю сторону
		canvas.DrawLine(
			transformedEdges.Last().X, transformedEdges.Last().Y,
			transformedEdges.First().X, transformedEdges.First().Y,
			new SKPaint { Color = SKColor.Parse("#000000") }
		);

		// Центр массы (центр фигуры)
		//canvas.DrawCircle(entity.Position.X, entity.Position.Y, 5, new SKPaint
		//{
		//	Color = SKColor.Parse("#003366")
		//});
	}
}