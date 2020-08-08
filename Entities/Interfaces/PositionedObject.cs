using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rockfan.Framework.Entities.Interfaces
{
    /// <summary>
    /// A Base object to derive from, when constructing objects which are positioned in world space
    /// </summary>
    public abstract class PositionedObject : IUpdateableEntity
    {
        #region Properties

        /// <summary>
        /// Gets or Sets a string value representing the name of this object
        /// </summary>
        public abstract string Name { get; set; }

        /// <summary>
        /// Gets or Sets a float value indicating the amount of time in seconds since the start of the engine services, of when this object was created
        /// </summary>
        public abstract float TimeCreated { get; protected set; }
        
        /// <summary>
        /// Gets or Sets a value indicating whether this object should update
        /// </summary>
        public bool EnableUpdates { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating the position in world space of this object along the x-axis
        /// </summary>
        public float X
        {
            get { return _Position.X; }
            set { SetPosition(value, Y, Z); }
        }

        /// <summary>
        /// Gets or Sets a value indicating the rotation in world space of this object along the x-axis
        /// </summary>
        public float XRotation
        {
            get { return _Rotation.X; }
            set { SetRotation(value, YRotation, ZRotation); }
        }

        /// <summary>
        /// Get or Sets a value indicating the scale of this object in world space along the x-axis
        /// </summary>
        public float XScale
        {
            get { return _Scale.X; }
            set { SetScale(value, YScale); }
        }

        /// <summary>
        /// Gets or Sets a value indicating the position in world space of this object along the y-axis
        /// </summary>
        public float Y
        {
            get { return _Position.Y; }
            set { SetPosition(X, value, Z); }
        }

        /// <summary>
        /// Gets or Sets a value indicating the rotation in world space of this object along the y-axis
        /// </summary>
        public float YRotation
        {
            get { return _Rotation.Y; }
            set { SetRotation(XRotation, value, ZRotation); }
        }

        /// <summary>
        /// Get or Sets a value indicating the scale of this object in world space along the y-axis
        /// </summary>
        public float YScale
        {
            get { return _Scale.Y; }
            set { SetScale(XScale, value); }
        }

        /// <summary>
        /// Gets or Sets a value indicating the position in world space of this object along the z-axis
        /// </summary>
        public float Z
        {
            get { return _Position.Z; }
            set { SetPosition(X, Y, value); }
        }

        /// <summary>
        /// Gets or Sets a value indicating the rotation in world space of this object along the z-axis
        /// </summary>
        public float ZRotation
        {
            get { return _Rotation.Z; }
            set { SetRotation(XRotation, YRotation, value); }
        }

        /// <summary>
        /// Gets a Matrix defining this object in world space
        /// </summary>
        public Matrix World { get; private set; }

        #endregion

        #region Methods

        #region Constructor

        /// <summary>
        /// Constructs a new instance of this object
        /// </summary>
        public PositionedObject()
        {
            EnableUpdates = true;
            _Scale = Vector2.One;
            _Position = Vector3.Zero;
            _Rotation = Vector3.Zero;

            World = Matrix.Identity;
            ScaleMatrix = Matrix.Identity;
            RotationXMatrix = Matrix.Identity;
            RotationYMatrix = Matrix.Identity;
            RotationZMatrix = Matrix.Identity;
            TranslationMatrix = Matrix.Identity;
        }

        #endregion

        /// <summary>
        /// Method used to update this object
        /// </summary>
        /// <param name="delta">the amount time in seconds since the last update</param>
        protected virtual void Update(float delta)
        {
            if (ScaleChanged || PositionChanged || RotationChanged)
            {
                Matrix.CreateScale(XScale, YScale, 1.0f, out ScaleMatrix);
                Matrix.CreateRotationX(MathHelper.ToRadians(XRotation), out RotationXMatrix);
                Matrix.CreateRotationY(MathHelper.ToRadians(YRotation), out RotationYMatrix);
                Matrix.CreateRotationZ(MathHelper.ToRadians(ZRotation), out RotationZMatrix);
                Matrix.CreateTranslation(ref _Position, out TranslationMatrix);

                World = ScaleMatrix * RotationXMatrix * RotationYMatrix * RotationZMatrix * TranslationMatrix;

                ScaleChanged = false;
                PositionChanged = false;
                RotationChanged = false;                
            }

            HasAlreadyUpdated = true;
        }

        /// <summary>
        /// Method used to set the scale of this object
        /// </summary>
        /// <param name="xScale">a float value indicating the value along the x-axis</param>
        /// <param name="yScale">a float value indicating the value along the y-axis</param>
        protected void SetScale(float xScale, float yScale)
        {
            ScaleChanged = XScale != xScale || YScale != yScale;

            if (ScaleChanged)
            {
                _Scale.X = xScale;
                _Scale.Y = yScale;
            }

            if (!HasAlreadyUpdated)
                ForceUpdate();
        }

        /// <summary>
        /// Method used to set the position of this object in world space
        /// </summary>
        /// <param name="x">a float value indicating the position of this object in world space along the x-axis</param>
        /// <param name="y">a float value indicating the position of this object in world space along the y-axis</param>
        /// <param name="z">a float value indicating the position of this object in world space along the z-axis</param>
        protected void SetPosition(float x, float y, float z)
        {
            PositionChanged = X != x || Y != y || Z != z;

            if (PositionChanged)
            {
                _Position.X = x;
                _Position.Y = y;
                _Position.Z = z;
            }

            if (!HasAlreadyUpdated)
                ForceUpdate();
        }

        /// <summary>
        /// Method used to set the rotation of this object in world space coordinates
        /// </summary>
        /// <param name="xRot">a float value indicating the rotation in world space along the x-axis</param>
        /// <param name="yRot">a float value indicating the rotation in world space along the y-axis</param>
        /// <param name="zRot">a float value indicating the rotation in world space along the z-axis</param>
        protected void SetRotation(float xRot, float yRot, float zRot)
        {
            RotationChanged = XRotation != xRot || YRotation != yRot || ZRotation != zRot;

            if (RotationChanged)
            {
                _Rotation.X = xRot % 360.0f;
                _Rotation.Y = yRot % 360.0f;
                _Rotation.Z = zRot % 360.0f;
            }

            if (!HasAlreadyUpdated)
                ForceUpdate();
        }

        /// <summary>
        /// Forces an update, before this object has had it's update method called by an outside source
        /// </summary>
        private void ForceUpdate()
        {
            if (ScaleChanged || PositionChanged || RotationChanged)
            {
                Matrix.CreateScale(XScale, YScale, 1.0f, out ScaleMatrix);
                Matrix.CreateRotationX(MathHelper.ToRadians(XRotation), out RotationXMatrix);
                Matrix.CreateRotationY(MathHelper.ToRadians(YRotation), out RotationYMatrix);
                Matrix.CreateRotationZ(MathHelper.ToRadians(ZRotation), out RotationZMatrix);
                Matrix.CreateTranslation(ref _Position, out TranslationMatrix);

                World = ScaleMatrix * RotationXMatrix * RotationYMatrix * RotationZMatrix * TranslationMatrix;

                ScaleChanged = false;
                PositionChanged = false;
                RotationChanged = false;
            }
        }

        // The interface method implementation for IUpdateableEntity.Update
        void IUpdateableEntity.Update(float delta)
        {
            if (this.EnableUpdates)
                this.Update(delta);
        }        

        #endregion

        private bool ScaleChanged;
        private bool PositionChanged;
        private bool RotationChanged;
        
        private Vector2 _Scale;
        private Vector3 _Position;
        private Vector3 _Rotation;

        private Matrix ScaleMatrix;
        private Matrix RotationXMatrix;
        private Matrix RotationYMatrix;
        private Matrix RotationZMatrix;
        private Matrix TranslationMatrix;

        private bool HasAlreadyUpdated;
    }
}
