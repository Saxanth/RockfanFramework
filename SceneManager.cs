using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Rockfan.Framework;
using Rockfan.Framework.Audio;
using Rockfan.Framework.Entities;
using Rockfan.Framework.Entities.Interfaces;
using Rockfan.Framework.Graphics;

namespace Rockfan.Framework
{
    public static class SceneManager
    {
        static SceneManager()
        {
            Scenes = new List<Scene>();
        }

        internal static void Update(float delta)
        {
            if (CurrentScene != null)
            {
                if (!LastSceneLoaded)
                {
                    CurrentScene.IntializeScene();
                    CurrentScene.Load();
                    CurrentScene.Activity(true);

                    LastSceneLoaded = true;
                }
                else CurrentScene.Update(delta);
            }
        }

        internal static void Draw()
        {
            if (CurrentScene != null)
                CurrentScene.Draw(null);
        }

        internal static void Remove(Scene scene)
        {
            if (scene == null)
                return;

            Scenes.Remove(scene);

            if (CurrentScene == scene)
                CurrentScene = Scenes.LastOrDefault();

            (scene as IDestroyable).Destroy();
        }

        public static void SetCurrentScene(Scene scene)
        {
            if (!Scenes.Contains(scene))
                Scenes.Add(scene);

            CurrentScene = scene;
        }

        public static void StartScene(string sceneType)
        {
            var assembly = Assembly.GetCallingAssembly();
            var assemblyType = assembly.GetType(sceneType, true, true);
            var scene = (Scene)Activator.CreateInstance(assemblyType, null);

            CurrentScene = scene;
            Scenes.Add(scene);

            if (EngineServices.GraphicsDevice != null)
            {
                scene.IntializeScene();
                scene.Load();
                scene.Activity(true);

                LastSceneLoaded = true;
            }
        }

        public static void StartScene(string sceneType, params object[] parameters)
        {
            var param = parameters.Length == 0 ? null : parameters;

            var assembly = Assembly.GetCallingAssembly();
            var assemblyType = assembly.GetType(sceneType, true, true);
            var scene = (Scene)Activator.CreateInstance(assemblyType, param);

            CurrentScene = scene;
            Scenes.Add(scene);

            if (EngineServices.GraphicsDevice != null)
            {
                scene.IntializeScene();
                scene.Load();
                scene.Activity(true);

                LastSceneLoaded = true;
            }
        }

        public static async Task<T> LoadSceneAsync<T>(string popupToLoad, ProgressChanged callback, params object[] parameters) where T : Scene
        {
            var assembly = Assembly.GetCallingAssembly();
            var assemblyTypes = assembly.GetType(popupToLoad, true, true);
            var scene = (T)Activator.CreateInstance(assemblyTypes, parameters);

            scene.IntializeScene();
            await scene.LoadAsync(callback);

            scene.Activity(true);
            Scenes.Add(scene);

            return scene;
        }

        private static bool LastSceneLoaded;
        private static Scene CurrentScene;
        private static List<Scene> Scenes;
    }
}
