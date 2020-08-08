using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Rockfan.Framework.Entities.Interfaces;

namespace Rockfan.Framework.Graphics
{
    public class Sprite : PositionedObject, IDrawableEntity
    {
        #region Properties

        /// <summary>
        /// Get or Sets a value indicating the name of this object
        /// </summary>
        public override string Name { get; set; }

        /// <summary>
        /// Get a float value indicating the amount of time in seconds since the initalization of the engine services
        /// </summary>
        public override float TimeCreated { get; protected set; }
        
        /// <summary>
        /// Gets or Sets a value indicating whether this object should be drawn
        /// </summary>
        public bool EnableDrawing { get; set; }

        /// <summary>
        /// Gets or Sets the alpha color component of this object
        /// </summary>
        public float Alpha
        {
            get { return ColorValues.W; }
            set { SetColor(Blue, Green, Red, value); }
        }

        /// <summary>
        /// Gets or Sets the blue color component of this object
        /// </summary>
        public float Blue
        {
            get { return ColorValues.X; }
            set { SetColor(value, Green, Red, Alpha); }
        }

        /// <summary>
        /// Gets or Sets the green color component of this object
        /// </summary>
        public float Green
        {
            get { return ColorValues.Y; }
            set { SetColor(Blue, value, Red, Alpha); }
        }

        /// <summary>
        /// Gets or Sets the red color component of this object
        /// </summary>
        public float Red
        {
            get { return ColorValues.Z; }
            set { SetColor(Blue, Green, value, Alpha); }
        }

        /// <summary>
        /// Gets or Sets the top position in screen space of the texture coordinate to use, when drawing the texture for this object
        /// </summary>
        public float TopTextureCoordinate
        {
            get { return TextureCoordinates.Y; }
            set { SetTextureCoordinates(LeftTextureCoordinate, value, RightTextureCoordinate, BottomTextureCoordinate); }
        }

        /// <summary>
        /// Gets or Sets the left position in screen space of the texture coordinate to use, when drawing the texture for this object
        /// </summary>
        public float LeftTextureCoordinate
        {
            get { return TextureCoordinates.X; }
            set { SetTextureCoordinates(value, TopTextureCoordinate, RightTextureCoordinate, BottomTextureCoordinate); }
        }

        /// <summary>
        /// Gets or Sets the right position in screen space of the texture coordinate to use, when drawing the texture for this object
        /// </summary>
        public float RightTextureCoordinate
        {
            get { return TextureCoordinates.Z; }
            set { SetTextureCoordinates(LeftTextureCoordinate, TopTextureCoordinate, value, BottomTextureCoordinate); }
        }

        /// <summary>
        /// Gets or Sets the bottom position in screen space of the texture coordinate to use, when drawing the texture for this object
        /// </summary>
        public float BottomTextureCoordinate
        {
            get { return TextureCoordinates.W; }
            set { SetTextureCoordinates(LeftTextureCoordinate, TopTextureCoordinate, RightTextureCoordinate, value); }
        }

        /// <summary>
        /// Gets or Sets the visible representation of this object
        /// </summary>
        public Texture2D Texture { get; set; }

        /// <summary>
        /// Gets the Vertices used in drawing this object
        /// </summary>
        public VertexPositionColorTexture[] VerticesForDrawing { get; private set; }

        #endregion

        #region Methods

        #region Constructor

        /// <summary>
        /// Constructs a new instance of this object
        /// </summary>
        /// <param name="texture">the visible representation of this object</param>
        public Sprite(Texture2D texture)
        {
            Texture = texture;
            _Color = Color.White;
            EnableDrawing = true;
            ColorValues = Vector4.One;
            TextureCoordinates = new Vector4(0, 0, 1, 1); // { left, top, right, bottom }
            VerticesForDrawing = new VertexPositionColorTexture[4];

            VerticesForDrawing[0].Position = new Vector3(-1f,  1f, 0); // This is Actually TopLeft
            VerticesForDrawing[1].Position = new Vector3( 1f,  1f, 0); // This is Acutally TopRight
            VerticesForDrawing[2].Position = new Vector3( 1f, -1f, 0); // This is Actually BottomRight
            VerticesForDrawing[3].Position = new Vector3(-1f, -1f, 0); // This is Actually BottomLeft

            VerticesForDrawing[0].Color = Color.White;
            VerticesForDrawing[1].Color = Color.White;
            VerticesForDrawing[2].Color = Color.White;
            VerticesForDrawing[3].Color = Color.White;

            VerticesForDrawing[0].TextureCoordinate = new Vector2(LeftTextureCoordinate, TopTextureCoordinate);
            VerticesForDrawing[1].TextureCoordinate = new Vector2(RightTextureCoordinate, TopTextureCoordinate);
            VerticesForDrawing[2].TextureCoordinate = new Vector2(RightTextureCoordinate, BottomTextureCoordinate);
            VerticesForDrawing[3].TextureCoordinate = new Vector2(LeftTextureCoordinate, BottomTextureCoordinate);

            Indicies = new int[] { 0, 1, 2, 2, 3, 0 };
        }

        #endregion

        /// <summary>
        /// Method used to optionally force an update on this object
        /// </summary>
        /// <param name="delta">a float value representing the amount of time since the last update</param>
        /// <param name="forceUpdate">a value indicating whether to ignore the "EnableUpdates" flag</param>
        public void Update(float delta, bool forceUpdate)
        {
            if (!forceUpdate && !EnableUpdates)
                return;

            Update(delta);
        }

        /// <summary>
        /// Method used to draw this object, using the supplied effect
        /// </summary>
        /// <param name="effect">an effect to use when drawing this object</param>
        public void Draw(Effect effect)
        {
            if (!EnableDrawing || effect == null || effect.GraphicsDevice == null)
                return;

            if (effect is BasicEffect)
            {
                var basicEffec = (BasicEffect)effect;
                basicEffec.Texture = this.Texture;
                basicEffec.World = this.World;

                effect.CurrentTechnique.Passes[0].Apply();
            }
            else if (effect is FretboardEffect)
            {
                var fbEffect = (FretboardEffect)effect;
                var needsUpdate = fbEffect.Texture != this.Texture || fbEffect.Alpha != this.Alpha || fbEffect.World != this.World;

                if (needsUpdate)
                {
                    fbEffect.Texture = this.Texture;
                    fbEffect.Alpha = this.Alpha;
                    fbEffect.World = this.World;

                    fbEffect.CurrentTechnique.Passes[0].Apply();
                }
            }            
            else
            {
                if (effect.Parameters["Texture"] != null)
                    effect.Parameters["Texture"].SetValue(this.Texture);

                if (effect.Parameters["Alpha"] != null)
                    effect.Parameters["Alpha"].SetValue(this.Alpha);

                if (effect.Parameters["World"] != null)
                    effect.Parameters["World"].SetValue(this.World);

                effect.CurrentTechnique.Passes[0].Apply();
            }

            effect.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, VerticesForDrawing, 0, 4, Indicies, 0, 2);
        }

        /// <summary>
        /// Method use to determine whether the bounds of this object is in the visible view space
        /// Remarks: Currently Not Implemented
        /// </summary>
        /// <exception cref="System.NotImplementedException">System.NotImplamentException</exception>
        /// <param name="frustum">an object which defines the visible view space</param>        
        public bool GetIsInView(BoundingFrustum frustum)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Method use to determine whether the bounds of this object is in the visible view space
        /// Remarks: Currently Not Implemented
        /// </summary>
        /// <exception cref="System.NotImplementedException">System.NotImplamentException</exception>
        /// <param name="view">an object which defines the "view matrix" in view space</param>
        /// /// <param name="view">an object which defines the "projection matrix" in view space</param>
        public bool GetIsInView(Matrix view, Matrix projection)
        {
            throw new NotImplementedException();
        }

        protected override void Update(float delta)
        {
            VerticesForDrawing[0].TextureCoordinate.X = LeftTextureCoordinate;
            VerticesForDrawing[0].TextureCoordinate.Y = TopTextureCoordinate;

            VerticesForDrawing[1].TextureCoordinate.X = RightTextureCoordinate;
            VerticesForDrawing[1].TextureCoordinate.Y = TopTextureCoordinate;

            VerticesForDrawing[2].TextureCoordinate.X = RightTextureCoordinate;
            VerticesForDrawing[2].TextureCoordinate.Y = BottomTextureCoordinate;

            VerticesForDrawing[3].TextureCoordinate.X = LeftTextureCoordinate;
            VerticesForDrawing[3].TextureCoordinate.Y = BottomTextureCoordinate;

            //if (ColorChanged)
            //{
            //    VerticesForDrawing[0].Color.PackedValue = _Color.PackedValue;
            //    VerticesForDrawing[1].Color.PackedValue = _Color.PackedValue;
            //    VerticesForDrawing[2].Color.PackedValue = _Color.PackedValue;
            //    VerticesForDrawing[3].Color.PackedValue = _Color.PackedValue;

            //    ColorChanged = false;
            //}

            base.Update(delta);
        }

        private void SetColor(float blue, float green, float red, float alpha)
        {
            var cBlue   = blue  > 1.0f ? 1.0f : blue  < 0.0f ? 0.0f : blue;
            var cGreen  = green > 1.0f ? 1.0f : green < 0.0f ? 0.0f : green;
            var cRed    = red   > 1.0f ? 1.0f : red   < 0.0f ? 0.0f : red;
            var cAlpha  = alpha > 1.0f ? 1.0f : alpha < 0.0f ? 0.0f : alpha;

            ColorChanged = ColorValues.X != cBlue || ColorValues.Y != cGreen || ColorValues.Z != cRed || ColorValues.W != cAlpha;

            if (ColorChanged)
            {
                _Color.PackedValue = (_Color.PackedValue & 0x00ffffff) | ((uint)(cAlpha * 255.0f) << 24);
                _Color.PackedValue = (_Color.PackedValue & 0xff00ffff) | ((uint)(cBlue  * 255.0f) << 16);
                _Color.PackedValue = (_Color.PackedValue & 0xffff00ff) | ((uint)(cGreen * 255.0f) << 8);
                _Color.PackedValue = (_Color.PackedValue & 0xffffff00) |  (uint)(cRed   * 255.0f);

                ColorValues.X = cBlue;
                ColorValues.Y = cGreen;
                ColorValues.Z = cRed;
                ColorValues.W = cAlpha;
            }
        }

        private void SetTextureCoordinates(float left, float top, float right, float bottom)
        {
            CoordinatesChanged = 
                TopTextureCoordinate != top || 
                LeftTextureCoordinate != left || 
                RightTextureCoordinate != right || 
                BottomTextureCoordinate != bottom;

            if (CoordinatesChanged)
            {
                TextureCoordinates.X = left;
                TextureCoordinates.Y = top;
                TextureCoordinates.Z = right;
                TextureCoordinates.W = bottom;
            }
        }

        #endregion

        private bool ColorChanged;
        private bool CoordinatesChanged;

        protected int[] Indicies;
        
        private Color _Color;
        private Vector4 ColorValues;
        private Vector4 TextureCoordinates;
    }
}
