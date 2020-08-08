using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Rockfan.Framework.Entities;
using Rockfan.Framework.Entities.Interfaces;
using Rockfan.Framework.Graphics;

namespace Rockfan.Framework
{
    public sealed class Camera : IUpdateableEntity
    {
        #region Properties

        public string Name { get; set; }

        public float TimeCreated { get; private set; }

        bool IUpdateableEntity.EnableUpdates { get { return true; } set { } }

        public float FieldOfView
        {
            get { return ViewVector.X; }
            set { SetPerspective(value, AspectRatio, NearPlane, FarPlane); }
        }

        public float AspectRatio
        {
            get { return ViewVector.Y; }
            set { SetPerspective(FieldOfView, value, NearPlane, FarPlane); }
        }

        public float NearPlane
        {
            get { return ViewVector.Z; }
            set { SetPerspective(FieldOfView, AspectRatio, value, FarPlane); }
        }

        public float FarPlane
        {
            get { return ViewVector.W; }
            set { SetPerspective(FieldOfView, AspectRatio, NearPlane, value); }
        }
        
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
        
        public Matrix View 
        { 
            get { return _View; } 
            private set { _View = value; } 
        }

        public Matrix Projection 
        { 
            get { return _Projection; } 
            private set { _Projection = value; } 
        }

        public Viewport Viewport { get; set; }

        #endregion

        public Camera()
        {
            _Position = new Vector3(0, 0, -10);
            _Rotation = Vector3.Zero;

            UpVector = Vector3.Up;
            OriginalUpVector = UpVector;

            ForwardVector = Vector3.Forward;
            OriginalForwardVector = ForwardVector;

            ViewVector = new Vector4(72.0f, 1.33f, 0.2f, 1000.0f);

            View = Matrix.CreateLookAt(_Position, ForwardVector, UpVector);
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(72.0f), 1.33f, 0.2f, 1000.0f);

            RotationMatrix = Matrix.Identity;
            RotationXMatrix = Matrix.Identity;
            RotationYMatrix = Matrix.Identity;
            RotationZMatrix = Matrix.Identity;

            Viewport = new Viewport(0, 0, EngineServices.GraphicsDevice.Viewport.Width, EngineServices.GraphicsDevice.Viewport.Height);
        }

        public void Update(float delta)
        {
            if (PerspectiveChanged)
            {
                Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FieldOfView), AspectRatio, NearPlane, FarPlane, out _Projection);
                PerspectiveChanged = false;
            }

            if (PositionChanged || RotationChanged)
            {
                if (RotationChanged)
                {
                    Matrix.CreateRotationX(MathHelper.ToRadians(XRotation), out RotationXMatrix);
                    Matrix.CreateRotationY(MathHelper.ToRadians(YRotation), out RotationYMatrix);
                    Matrix.CreateRotationZ(MathHelper.ToRadians(ZRotation), out RotationZMatrix);

                    RotationMatrix = RotationXMatrix * RotationYMatrix * RotationZMatrix;
                }

                ForwardVector = _Position + Vector3.Transform(OriginalForwardVector, RotationMatrix);
                UpVector = Vector3.Transform(OriginalUpVector, RotationMatrix);
                Matrix.CreateLookAt(ref _Position, ref ForwardVector, ref UpVector, out _View);                

                PositionChanged = false;
                RotationChanged = false;
            }

            HasUpdatedOnce = true;
        }

        public float GetYScaleAtDistance(float value)
        {
            var distance = (value - _Position.Z) / 100.0f;
            var fov = (FieldOfView / 2.0f) * AspectRatio;

            return 100.0f * (float)Math.Tan(fov) * distance * Vector3.Forward.Z / AspectRatio;
        }

        public float GetXScaleAtDistance(float value)
        {
            var distance = (value - _Position.Z) / 100.0f;
            var fov = (FieldOfView / 2.0f) * AspectRatio;

            return 100.0f * (float)Math.Tan(fov) * distance * Vector3.Forward.Z;
        }

        private void ForcedUpdate()
        {
            if (!HasUpdatedOnce)
            {
                if (PerspectiveChanged)
                {
                    Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FieldOfView), AspectRatio, NearPlane, FarPlane, out _Projection);
                    PerspectiveChanged = false;
                }

                if (PositionChanged || RotationChanged)
                {
                    if (RotationChanged)
                    {
                        Matrix.CreateRotationX(MathHelper.ToRadians(XRotation), out RotationXMatrix);
                        Matrix.CreateRotationY(MathHelper.ToRadians(YRotation), out RotationYMatrix);
                        Matrix.CreateRotationZ(MathHelper.ToRadians(ZRotation), out RotationZMatrix);

                        RotationMatrix = RotationXMatrix * RotationYMatrix * RotationZMatrix;
                    }

                    ForwardVector = _Position + Vector3.Transform(OriginalForwardVector, RotationMatrix);
                    UpVector = Vector3.Transform(OriginalUpVector, RotationMatrix);
                    Matrix.CreateLookAt(ref _Position, ref ForwardVector, ref UpVector, out _View);

                    PositionChanged = false;
                    RotationChanged = false;
                }
            }
        }

        private void SetPerspective(float fieldOfView, float aspectRatio, float nearPlane, float farPlane)
        {
            var cFieldOfView = MathHelper.Clamp(fieldOfView, 0.1f, 179.5f);
            var cNearPlane = MathHelper.Max(0.2f, nearPlane);

            PerspectiveChanged = FieldOfView != cFieldOfView || AspectRatio != aspectRatio || NearPlane != cNearPlane || FarPlane != farPlane;

            if (PerspectiveChanged)
            {
                ViewVector.X = cFieldOfView;
                ViewVector.Y = aspectRatio;
                ViewVector.Z = cNearPlane;
                ViewVector.W = farPlane;
            }

            if (!HasUpdatedOnce)
                ForcedUpdate();
        }

        /// <summary>
        /// Method used to set the position of this object in world space
        /// </summary>
        /// <param name="x">a float value indicating the position of this object in world space along the x-axis</param>
        /// <param name="y">a float value indicating the position of this object in world space along the y-axis</param>
        /// <param name="z">a float value indicating the position of this object in world space along the z-axis</param>
        private void SetPosition(float x, float y, float z)
        {
            PositionChanged = X != x || Y != y || Z != z;

            if (PositionChanged)
            {
                _Position.X = x;
                _Position.Y = y;
                _Position.Z = z;
            }

            if (!HasUpdatedOnce)
                ForcedUpdate();
        }

        /// <summary>
        /// Method used to set the rotation of this object in world space coordinates
        /// </summary>
        /// <param name="xRot">a float value indicating the rotation in world space along the x-axis</param>
        /// <param name="yRot">a float value indicating the rotation in world space along the y-axis</param>
        /// <param name="zRot">a float value indicating the rotation in world space along the z-axis</param>
        private void SetRotation(float xRot, float yRot, float zRot)
        {
            RotationChanged = XRotation != xRot || YRotation != yRot || ZRotation != zRot;

            if (RotationChanged)
            {
                _Rotation.X = xRot % 360.0f;
                _Rotation.Y = yRot % 360.0f;
                _Rotation.Z = zRot % 360.0f;
            }

            if (!HasUpdatedOnce)
                ForcedUpdate();
        }
        
        private bool PositionChanged;
        private bool RotationChanged;
        private bool PerspectiveChanged;

        private Vector3 _Position;
        private Vector3 _Rotation;

        private Vector3 UpVector;
        private Vector3 ForwardVector;
        private Vector3 OriginalUpVector;
        private Vector3 OriginalForwardVector;
        
        private Vector4 ViewVector;

        private Matrix _View;
        private Matrix _Projection;

        private Matrix RotationMatrix;
        private Matrix RotationXMatrix;
        private Matrix RotationYMatrix;
        private Matrix RotationZMatrix;

        private bool HasUpdatedOnce;
    }
}
