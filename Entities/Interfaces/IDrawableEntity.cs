using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rockfan.Framework.Entities.Interfaces
{
    public interface IDrawableEntity : IUpdateableEntity
    {
        float X { get; set; }
        float Y { get; set; }
        float Z { get; set; }
        
        bool EnableDrawing { get; set; }

        void Draw(Effect effect);

        bool GetIsInView(BoundingFrustum frustum);
        bool GetIsInView(Matrix view, Matrix projection);
    }
}
