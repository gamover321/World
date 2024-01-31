using SkiaSharp;
using World.Engine.My2dWorld.Primitives;

namespace World.Engine.Render.Primitives
{
    public class TrailEntityRender : BaseEntityRender
    {
        private PhysicsBaseEntity entity { get; set; }

        private Queue<Vector.Vector> prevPositons { get; set; } = new();

        public TrailEntityRender(PhysicsBaseEntity entity) : base(entity)
        {
            this.entity = entity;
        }

        public override void Render(SKCanvas canvas)
        {
            RenderPrevPositions(canvas);
            //RenderCircleRadius(canvas);

            base.Render(canvas);

            RenderAcceleration(canvas);
        }

        private void RenderCircleRadius(SKCanvas canvas)
        {
            canvas.DrawCircle(entity.Position.X, entity.Position.Y, entity.GetRadius(), new SKPaint
            {
                Color = SKColor.Parse("#00FFFF")
            });
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

        private void RenderPrevPositions(SKCanvas canvas)
        {
            foreach (var positon in prevPositons)
            {
                canvas.DrawCircle(positon.X, positon.Y, 5, new SKPaint
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
    }
}
