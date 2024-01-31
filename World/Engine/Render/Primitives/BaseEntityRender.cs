using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using World.Engine.My2dWorld.Primitives;

namespace World.Engine.Render.Primitives
{
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

            var posX = entity.Position.X;
            var posY = entity.Position.Y;

            foreach (var edge in entity.Edges)
            {
                if (prevX == null || prevY == null)
                {
                    prevX = edge.X;
                    prevY = edge.Y;
                    continue;
                }

                canvas.DrawLine(prevX.Value + posX, prevY.Value + posY, edge.X + posX, edge.Y + posY, new SKPaint { Color = SKColor.Parse("#000000") });

                prevX = edge.X;
                prevY = edge.Y;
            }
            canvas.DrawLine(entity.Edges.Last().X + posX, entity.Edges.Last().Y + posY,
                entity.Edges.First().X + posX, entity.Edges.First().Y + posY, new SKPaint { Color = SKColor.Parse("#000000") });


            canvas.DrawCircle(entity.Position.X, entity.Position.Y, 10, new SKPaint
            {
                Color = SKColor.Parse("##003366")
            });
        }
    }
}
