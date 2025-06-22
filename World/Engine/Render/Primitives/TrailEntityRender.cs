using SkiaSharp;
using World.Engine.My2dWorld;
using World.Engine.My2dWorld.Primitives;

namespace World.Engine.Render.Primitives
{
    public class TrailEntityRender : BaseEntityRender
    {
        private PhysicsBaseEntity entity { get; set; }

        private Queue<Engine.Primitives.MyVector> prevPositons { get; set; } = new();
        private Queue<Engine.Primitives.MyVector> prevCollisionPoints { get; set; } = new();

		public TrailEntityRender(PhysicsBaseEntity entity) : base(entity)
        {
            this.entity = entity;
        }

        public override void Render(SKCanvas canvas, Camera2d camera2d)
        {
	        base.Render(canvas, camera2d);

			//RenderPrevPositions(canvas);
			//RenderPrevCollisions(canvas);

			//RenderCircleRadius(canvas);
			//RenderAcceleration(canvas);
			//RenderVelocity(canvas);
			//RenderPosition(canvas);
            //RenderNormals(canvas);

        }

        private void RenderCircleRadius(SKCanvas canvas)
        {
            canvas.DrawCircle(entity.Position.X, entity.Position.Y, entity.GetRadius(), new SKPaint
            {
                Color = SKColor.Parse("#00FFFF")
            });
        }

        private void RenderNormals(SKCanvas canvas)
        {

	        var normals = entity.GetNormals(PhysicsContext.Actual);
	        foreach (var normal in normals)
	        {
				canvas.DrawLine(entity.Position.X, entity.Position.Y,
					entity.Position.X + normal.X * 30, entity.Position.Y + normal.Y*30, new SKPaint
					{
						Color = SKColor.Parse("#FF00FF")
					});
			}
        }

		private void RenderAcceleration(SKCanvas canvas)
        {
            var text = $"{entity.Acceleration.X:0.0} {entity.Acceleration.Y:0:0}";
            canvas.DrawText(text, entity.Position.X, entity.Position.Y - entity.GetRadius(), new SKPaint()
            {
                Color = SKColor.Parse("#000000")
            });

            canvas.DrawLine(entity.Position.X, entity.Position.Y,
                entity.Position.X + entity.Acceleration.X * 10000, entity.Position.Y + entity.Acceleration.Y * 10000, new SKPaint
                {
                    Color = SKColor.Parse("#FF00FF")
                });
        }

        private void RenderVelocity(SKCanvas canvas)
        {
	        //var text = $"{entity.Velocity.X:0.0} {entity.Velocity.Y:0:0}";
	        //canvas.DrawText(text, entity.Position.X, entity.Position.Y - entity.GetRadius(), new SKPaint()
	        //{
		    //    Color = SKColor.Parse("#000000")
	        //});

	        canvas.DrawLine(entity.Position.X, entity.Position.Y,
		        entity.Position.X + entity.Velocity.X * 20, entity.Position.Y + entity.Velocity.Y * 20, new SKPaint
		        {
			        Color = SKColor.Parse("#FFAAFF")
		        });
        }

		private void RenderPosition(SKCanvas canvas)
        {
	        var text = $"{entity.Name} ({entity.Position.X:0}:{entity.Position.Y:0})";
	        canvas.DrawText(text, entity.Position.X, entity.Position.Y + entity.GetRadius(), new SKPaint()
	        {
		        Color = SKColor.Parse("#000000")
	        });

	        //canvas.DrawLine(entity.Position.X, entity.Position.Y,
		    //    entity.Position.X + entity.Acceleration.X * 10000, entity.Position.Y + entity.Acceleration.Y * 10000, new SKPaint
		    //    {
			//        Color = SKColor.Parse("#FF00FF")
		    //    });
        }

		private void RenderPrevPositions(SKCanvas canvas)
        {
            foreach (var position in prevPositons)
            {
                canvas.DrawCircle(position.X, position.Y, 5, new SKPaint
                {
                    Color = SKColor.Parse("#003366")
                });
            }
            prevPositons.Enqueue(entity.Position.Copy());
            if (prevPositons.Count > 100)
            {
                prevPositons.Dequeue();
            }
        }

        private void RenderPrevCollisions(SKCanvas canvas)
        {
	        foreach (var prevPoint in prevCollisionPoints)
	        {
		        canvas.DrawCircle(prevPoint.X, prevPoint.Y, 3, new SKPaint
		        {
			        Color = SKColor.Parse("#000000")
		        });
	        }

	        foreach (var point in entity.CollisionPoints)
	        {
		        prevCollisionPoints.Enqueue(point.Copy());
			}
			
			if (prevCollisionPoints.Count > 100)
	        {
		        prevCollisionPoints.Dequeue();
	        }
        }
	}
}
