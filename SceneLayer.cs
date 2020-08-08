using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Rockfan.Framework;
using Rockfan.Framework.Audio;
using Rockfan.Framework.Entities;
using Rockfan.Framework.Entities.Interfaces;
using Rockfan.Framework.Graphics;

namespace Rockfan.Framework
{
    public sealed class SceneLayer : IDrawableEntity, IDestroyable
    {
        #region Properties

        public string Name { get; set; }
        public float TimeCreated { get; private set; }

        public bool EnableUpdates { get; set; }
        public bool EnableDrawing { get; set; }
        public bool EnableSorting { get; set; }

        float IDrawableEntity.X { get { return 0; } set { } }
        float IDrawableEntity.Y { get { return 0; } set { } }
        float IDrawableEntity.Z { get { return 0; } set { } }

        public Camera Camera { get; set; }
        public SortType SortType { get; set; }

        #endregion

        /// <summary>
        /// Constructs a new instance of this object
        /// </summary>
        /// <param name="camera">the Camera object to associate with this layer</param>
        public SceneLayer(string name, Camera camera)
        {
            this.Name = name;
            this.TimeCreated = EngineServices.TotalGameTime;
            this.Camera = camera;
            this.EnableUpdates = true;
            this.EnableDrawing = true;

            EntityCollection = new List<IEntity>();
            DestroyableCollection = new List<IDestroyable>();
            DrawableCollection = new List<IDrawableEntity>();
            UpdateableCollection = new List<IUpdateableEntity>();
        }

        internal void Add<T>(T entity) where T : IEntity
        {
            if (entity == null)
                return;

            var update = entity as IUpdateableEntity;
            var drawable = entity as IDrawableEntity;

            EntityCollection.Add(entity);

            if (update != null)
                UpdateableCollection.Add(update);

            if (drawable != null)
                DrawableCollection.Add(drawable);
        }

        internal void Remove<T>(T entity) where T : IEntity
        {
            if (entity == null)
                return;

            var entityType = entity.GetType();

            EntityCollection.Remove(entity);

            if (entityType == typeof(IUpdateableEntity))
                UpdateableCollection.Remove((IUpdateableEntity)entity);

            if (entityType == typeof(IDrawableEntity))
                DrawableCollection.Remove((IDrawableEntity)entity);
        }

        internal void Update(float delta)
        {
            if (UpdateableCollection.Count == 0 || !EnableUpdates)
                return;

            CameraPosition.X = Camera.X;
            CameraPosition.Y = Camera.Y;
            CameraPosition.Z = Camera.Z;

            foreach (var entity in UpdateableCollection)
                entity.Update(delta);

            if (EnableSorting)
            {
                if (SortType == SortType.AwayFromCamera)
                {
                    var leftDistance = 0.0f;
                    var rightDistance = 0.0f;

                    DrawableCollection.Sort((left, right) => {

                        leftDistance = 0.0f;
                        rightDistance = 0.0f;

                        DrawablePosition.X = left.X;
                        DrawablePosition.Y = left.Y;
                        DrawablePosition.Z = left.Z;

                        DrawableComparerPosition.X = right.X;
                        DrawableComparerPosition.Y = right.Y;
                        DrawableComparerPosition.Z = right.Z;

                        Vector3.Distance(ref CameraPosition, ref DrawablePosition, out leftDistance);
                        Vector3.Distance(ref CameraPosition, ref DrawableComparerPosition, out rightDistance);

                        return leftDistance < rightDistance ? -1 : leftDistance > rightDistance ? 1 : 0;
                    });
                }
            }
        }

        internal void Draw(Effect effect)
        {
            if (!EnableDrawing)
                return;

            bool IsReverseSort = (SortType &= SortType.ReverseSort) == SortType.ReverseSort;

            if (EnableSorting && IsReverseSort)
            {
                for (int i = DrawableCollection.Count - 1; i >= 0; i--)
                {
                    DrawableCollection[i].Draw(effect);
                }   
            }
            else for (int i = 0; i < DrawableCollection.Count; i++)
            {
                DrawableCollection[i].Draw(effect);
            }
                    
        }

        internal void Destroy()
        {
            foreach (var entity in DestroyableCollection)
                entity.Destroy();

            for (int i = EntityCollection.Count - 1; i >= 0; i--)
            {
                var entity = EntityCollection[i];

                EntityCollection.Remove(entity);
                DrawableCollection.Remove(entity as IDrawableEntity);
                DestroyableCollection.Remove(entity as IDestroyable);
                UpdateableCollection.Remove(entity as IUpdateableEntity);
            }

            CameraPosition = Vector3.Zero;
            DrawablePosition = Vector3.Zero;
            DrawableComparerPosition = Vector3.Zero;
        }

        void IUpdateableEntity.Update(float delta)
        {
            this.Update(delta);
        }

        void IDrawableEntity.Draw(Effect effect)
        {
            this.Draw(effect);
        }

        bool IDrawableEntity.GetIsInView(BoundingFrustum frustum)
        {
            return true;
        }

        bool IDrawableEntity.GetIsInView(Matrix view, Matrix projection)
        {
            return true;
        }

        void IDestroyable.Destroy()
        {
            this.Destroy();
        }

        private Vector3 CameraPosition;
        private Vector3 DrawablePosition;
        private Vector3 DrawableComparerPosition;

        private List<IEntity> EntityCollection;
        private List<IDestroyable> DestroyableCollection;
        private List<IDrawableEntity> DrawableCollection;
        private List<IUpdateableEntity> UpdateableCollection;
    }
}
