using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Rockfan.Framework.Audio;
using Rockfan.Framework.Entities;
using Rockfan.Framework.Entities.Interfaces;
using Rockfan.Framework.Graphics;

using System.Reflection;

namespace Rockfan.Framework
{
    public abstract class Scene : IDrawableEntity, IDestroyable
    {
        #region Properties

        public float TimeCreated { get; private set; }

        public BasicEffect DefaultEffect { get; set; }

        /// <summary>
        /// Gets the name of the content manager, this object uses
        /// </summary>
        public string ContentManagersName { get; private set; }

        public Camera Camera { get; private set; }

        public bool IsLoaded { get; private set; }

        public bool EnableUpdates { get; set; }

        public bool EnableDrawing { get; set; }

        string IEntity.Name { get { return string.Empty; } }

        float IDrawableEntity.X { get { return 0; } set { } }
        float IDrawableEntity.Y { get { return 0; } set { } }
        float IDrawableEntity.Z { get { return 0; } set { } }

        #endregion

        #region Methods

        #region Constructor(s)

        /// <summary>
        /// Constructs a new instance of this object
        /// </summary>
        /// <param name="contentManagersName">a string value representing the name of the contentMangaer to use, when loading content</param>
        public Scene(string contentManagersName)
        {
            this.EnableDrawing = true;
            this.EnableUpdates = true;

            this.Camera = new Camera() { Z = 40.0f };
            this.ContentManagersName = contentManagersName;            
            this.DefaultLayer = new SceneLayer("DefaultLayer", Camera);

            this.Popups = new List<Scene>();
            this.Layers = new List<SceneLayer>();            
        }

        #endregion

        protected virtual void Intialize() { }

        /// <summary>
        /// Method called when its time to load
        /// </summary>
        public virtual void Load() { IsLoaded = true; }        

        public abstract void Activity(bool isFirstCall);

        protected abstract void Destroy();

        public void AddLayer(SceneLayer layer)
        {

        }

        public SceneLayer AddLayer(string name)
        {

            return AddLayer(name, Camera);
        }

        public SceneLayer AddLayer(string name, Camera camera)
        {

            return default(SceneLayer);
        }

        public T LoadPopup<T>(string popupToLoad) where T : Scene
        {
            var assembly = Assembly.GetCallingAssembly();
            var assemblyTypes = assembly.GetType(popupToLoad, true, true);
            var scene = (T)Activator.CreateInstance(assemblyTypes, new object[] { ContentManagersName });

            scene.Intialize();
            scene.Load();
            scene.Activity(true);

            Popups.Add(scene);
            return scene;           
        }

        public void Add<T>(T entity) where T : IEntity
        {
            Add<T>(entity, DefaultLayer);
        }

        public void Add<T>(T entity, SceneLayer layer) where T : IEntity
        {
            if (layer != null)
                layer.Add<T>(entity);
        }

        public void Remove<T>(T entity) where T : IEntity
        {
            DefaultLayer.Remove<T>(entity);

            foreach (var layer in Layers)
                layer.Remove<T>(entity);
        }

        public void MoveToScene(string sceneName)
        {
            SceneManager.Remove(this);

            if (!string.IsNullOrEmpty(sceneName))
                SceneManager.StartScene(sceneName);
        }

        /// <summary>
        /// Method called when the caller wants to load this object asyncronously
        /// </summary>
        public virtual Task LoadAsync(ProgressChanged callback) 
        {
            this.IsLoaded = true;
            return Task.Run(() => { }); 
        }

        public async Task<T> LoadPopupAsync<T>(string popupToLoad, ProgressChanged callback) where T : Scene
        {
            var assembly = Assembly.GetCallingAssembly();
            var assemblyTypes = assembly.GetType(popupToLoad, true, true);
            var scene = (T)Activator.CreateInstance(assemblyTypes, new object[] { ContentManagersName });

            scene.Intialize();
            await scene.LoadAsync(callback);

            scene.Activity(true);
            Popups.Add(scene);

            return scene;
        }

        internal void IntializeScene()
        {
            this.Intialize();
        }

        internal void Update(float delta)
        {
            if (!EnableUpdates)
                return;

            Activity(false);
            DefaultLayer.Update(delta);

            foreach (var layer in Layers)
                layer.Update(delta);

            foreach (var popup in Popups)
                popup.Update(delta);
        }

        internal void Draw(Effect effect)
        {
            if (!EnableDrawing)
                return;

            if (DefaultEffect == null)
            {
                DefaultEffect = new BasicEffect(EngineServices.GraphicsDevice);
                DefaultEffect.TextureEnabled = true;
                
                return;
            }

            EngineServices.GraphicsDevice.Viewport = Camera.Viewport;

            DefaultEffect.View = Camera.View;
            DefaultEffect.Projection = Camera.Projection;
            DefaultEffect.CurrentTechnique.Passes[0].Apply();

            DefaultLayer.Draw(DefaultEffect);

            foreach (var layer in Layers)
            {
                layer.Draw(DefaultEffect);
            }   

            foreach (var popup in Popups)
            {
                popup.Draw(DefaultEffect);
            }                
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

            if (!string.IsNullOrEmpty(ContentManagersName) && !string.IsNullOrWhiteSpace(ContentManagersName))
                EngineServices.RemoveContentManager(ContentManagersName);
        }
        
        #endregion

        private SceneLayer DefaultLayer;

        protected List<Scene> Popups;
        protected List<SceneLayer> Layers;        
    }
}
