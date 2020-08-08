using System;
using System.Collections.Generic;
using System.IO;
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

using XnaContent = Microsoft.Xna.Framework.Content.ContentManager;

namespace Rockfan.Framework.Content
{
    public sealed class ContentManager : IEntity, IDestroyable
    {
        public string Name { get; private set; }
        public float TimeCreated { get; private set; }

        public ContentManager(GraphicsDevice device, string name)
        {
            this.Name = name;
            this.GraphicsDevice = device;
            this.SyncContext = new object();
            this.TimeCreated = EngineServices.TotalGameTime;

            ContentDictionary = new Dictionary<string, object>();
        }

        public Texture2D LoadTexture(string fileLocation)
        {
            if (!File.Exists(fileLocation))
                throw new FileNotFoundException(string.Format("{0} cannot be found in directory: {1}", Path.GetFileNameWithoutExtension(fileLocation), Path.GetDirectoryName(fileLocation)));

            var texture = default(object);

            lock (SyncContext)
            {
                if (ContentDictionary.TryGetValue(fileLocation, out texture) && texture != null)
                    return (Texture2D)texture;
            }

            var fStream = GetFileStream(fileLocation);
            texture = Texture2D.FromStream(GraphicsDevice, fStream);

            lock(SyncContext)
                ContentDictionary.Add(fileLocation, texture);

            return (Texture2D)texture;
        }

        //public SoundFX LoadSound(string fileLocation)
        //{
        //    if (!File.Exists(fileLocation))
        //        throw new FileNotFoundException(string.Format("{0} cannot be found in directory: {1}", Path.GetFileNameWithoutExtension(fileLocation), Path.GetDirectoryName(fileLocation)));

        //    var sound = default(object);

        //    lock (SyncContext)
        //    {
        //        if (ContentDictionary.TryGetValue(fileLocation, out sound) && sound != null)
        //            return (SoundFX)sound;
        //    }

        //    lock (SyncContext)
        //    {
        //        sound = new SoundFX(fileLocation, 64);
        //        ContentDictionary.Add(fileLocation, sound);
        //    }

        //    return (SoundFX)sound;
        //}

        /// <summary>
        /// An Asynchronous way to load textures, with a callback to progress changes
        /// </summary>
        /// <param name="fileLocation">a string value representing a location on disk to load from</param>
        /// <param name="callback">a callback value for progress change notifications</param>        
        public async Task<Texture2D> LoadTextureAsync(string fileLocation, ProgressChanged callback)
        {
            if (!File.Exists(fileLocation))
                throw new FileNotFoundException(string.Format("{0} cannot be found in directory: {1}", Path.GetFileNameWithoutExtension(fileLocation), Path.GetDirectoryName(fileLocation)));

            var task = Task<Texture2D>.Run(() =>
                {
                    var texture = LoadTexture(fileLocation);
                    
                    if (callback != null)
                        callback.Invoke(this, 100.0f);

                    return texture;
                });

            return await task;
        }

        private Stream GetFileStream(string fileLocation)
        {
            return File.OpenRead(fileLocation);
        }

        internal void Destroy()
        {
            foreach (var fileLocation in ContentDictionary.Keys)
            {
                var item = ContentDictionary[fileLocation];

                if (item is Texture2D)
                {
                    var texture = (Texture2D)item;
                    texture.Dispose();
                }
            }
        }

        void IDestroyable.Destroy()
        {
            this.Destroy();
        }

        private object SyncContext;
        private GraphicsDevice GraphicsDevice;
        private Dictionary<string, object> ContentDictionary;
    }
}
