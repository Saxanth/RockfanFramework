using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rockfan.Framework.Audio;
using Rockfan.Framework.Content;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rockfan.Framework
{
    public sealed class EngineServices :  IGameComponent, IUpdateable, IDrawable
    {
        #region Properties

        bool IUpdateable.Enabled
        {
            get { return true; }
        }

        bool IDrawable.Visible
        {
            get { return true; }
        }

        int IUpdateable.UpdateOrder
        {
            get { return 0; }
        }

        int IDrawable.DrawOrder
        {
            get { return 0; }
        }

        event EventHandler<EventArgs> IUpdateable.EnabledChanged
        {
            add { EnabledChanged += value; }
            remove { EnabledChanged -= value; }
        }

        event EventHandler<EventArgs> IUpdateable.UpdateOrderChanged
        {
            add { UpdateOrderChanged += value; }
            remove { UpdateOrderChanged -= value; }
        }

        event EventHandler<EventArgs> IDrawable.DrawOrderChanged
        {
            add { DrawOrderChanged += value; }
            remove { DrawOrderChanged -= value; }
        }

        event EventHandler<EventArgs> IDrawable.VisibleChanged
        {
            add { VisibleChanged += value; }
            remove { VisibleChanged -= value; }
        }

        public static float TotalGameTime { get; private set; }
        public static float ElapsedGameTime { get; private set; }
        
        public static GraphicsDevice GraphicsDevice { get; private set; }

        public static Microsoft.Xna.Framework.Content.ContentManager ContentManager { get; private set; }

        public static int ClientHeight
        {
            get
            {
                if (GraphicsDevice == null)
                    return 0;

                return GraphicsDevice.PresentationParameters.BackBufferHeight;
            }
        }

        public static int ClientWidth
        {
            get
            {
                if (GraphicsDevice == null)
                    return 0;

                return GraphicsDevice.PresentationParameters.BackBufferWidth;
            }
        }

        #endregion

        static EngineServices()
        {
            ClearColor = Color.Black;
            ContentManagerCollection = new List<ContentManager>();
        }

        private EngineServices(Game game)
        {
            game.Components.Add(this);
        }

        public static void Initialize(Game game)
        {
            if (Services != null)
                return;

            Game = game;
            Services = new EngineServices(game);
            ContentManager = Game.Content;
        }

        public static ContentManager GetContentManager(string contentManagersName)
        {
            foreach (var contentManager in ContentManagerCollection)
            {
                if (contentManager.Name == contentManagersName)
                    return contentManager;
            }

            var manager = new ContentManager(GraphicsDevice, contentManagersName);
            ContentManagerCollection.Add(manager);

            return manager;
        }

        internal static void RemoveContentManager(string contentManagersName)
        {
            for (int i = ContentManagerCollection.Count - 1; i >= 0; i--)
            {
                var manager = ContentManagerCollection[i];

                if (manager.Name == contentManagersName)
                {
                    manager.Destroy();
                    ContentManagerCollection.Remove(manager);
                }
            }
        }

        void IGameComponent.Initialize()
        {
            var manager = Game.Services.GetService(typeof(IGraphicsDeviceManager));

            if (manager != null)
                GraphicsDevice = ((GraphicsDeviceManager)manager).GraphicsDevice;

            else throw new NullReferenceException("GraphicsDeviceManager is null");

            var blendState = new BlendState();
            blendState.AlphaBlendFunction = BlendFunction.Add;
            blendState.AlphaDestinationBlend = Blend.SourceAlpha;
            blendState.AlphaSourceBlend = Blend.InverseSourceAlpha;

            blendState.ColorBlendFunction = BlendFunction.Add;
            blendState.ColorDestinationBlend = Blend.InverseSourceAlpha;
            blendState.ColorSourceBlend = Blend.SourceAlpha;

            GraphicsDevice.BlendState = blendState;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.SamplerStates[0] = new SamplerState() { AddressU = TextureAddressMode.Wrap, AddressV = TextureAddressMode.Wrap, AddressW = TextureAddressMode.Wrap, Filter = TextureFilter.Linear, MaxAnisotropy = 16 };

            Timer = new Stopwatch();
        }

        void IUpdateable.Update(GameTime gameTime)
        {
            if (!Timer.IsRunning)
                Timer.Start();

            ElapsedGameTime = ((Timer.ElapsedTicks / (float)Stopwatch.Frequency) ) - TotalGameTime;
            TotalGameTime += ElapsedGameTime;

            // AudioEngine.Update(ElapsedGameTime);
            SceneManager.Update(ElapsedGameTime);
        }

        void IDrawable.Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(ClearColor);

            // We are drawing with a null effect, cause the scene will manage it's own effects
            SceneManager.Draw();
        }

        private static Color ClearColor;
        private static Game Game;
        private static EngineServices Services;
        private static Stopwatch Timer;
        
        event EventHandler<EventArgs> EnabledChanged;
        event EventHandler<EventArgs> VisibleChanged;
        event EventHandler<EventArgs> UpdateOrderChanged;
        event EventHandler<EventArgs> DrawOrderChanged;

        private static List<ContentManager> ContentManagerCollection;
    }
}
