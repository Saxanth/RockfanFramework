using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rockfan.Framework.Graphics
{
    public sealed class TextureInfo
    {
        public Texture2D Texture { get; set; }

        public float TopTextureCoordinate { get; set; }
        public float LeftTextureCoordinate { get; set; }
        public float RightTextureCoordinate { get; set; }
        public float BottomTextureCoordinate { get; set; }

        /// <summary>
        /// Constructs a new instance of this object
        /// </summary>
        public TextureInfo()
        {
            TopTextureCoordinate = 0.0f;
            LeftTextureCoordinate = 0.0f;
            RightTextureCoordinate = 1.0f;
            BottomTextureCoordinate = 1.0f;
        }
    }
}
