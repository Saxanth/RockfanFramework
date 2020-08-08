using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using Rockfan.Framework;
using Rockfan.Framework.Audio;
using Rockfan.Framework.Content;
using Rockfan.Framework.Graphics;

namespace Rockfan.Framework
{
    public sealed class Theme
    {
        #region Properties

        private const string ThemeCacheLocation = @"Content\Themes\themes.rtc";
        private const string ThemeExtension = ".rft";

        public static string Name { get; private set; }
        public static string Alias { get; private set; }

        public static List<ThemeInfo> Themes { get; private set; }

        #endregion

        static Theme()
        {
            SoundSync = new object();
            Themes = new List<ThemeInfo>();
            Sounds = new Dictionary<string, SoundFX>();
            Textures = new Dictionary<string, TextureInfo>();
            AnimatedEntities = new Dictionary<string, AnimatedSprite>();
            ContentManager = new Content.ContentManager(EngineServices.GraphicsDevice, "_System_ThemeContent");                   
        }

        public static void LoadTheme(string themeName)
        {
            if (Themes.Count == 0)
                LoadThemeCache();

            foreach (var theme in Themes)
            {
                if (string.Compare(theme.Name, themeName, true) == 0)
                    LoadThemeFromInfo(theme);

                else if (string.Compare(theme.Alias, themeName, true) == 0)
                    LoadThemeFromInfo(theme);
            }
        }

        public static void PlaySoundFromName(string soundName)
        {
            Task.Run(() =>
                {
                    lock (SoundSync)
                    {
                        if (!Sounds.ContainsKey(soundName))
                            return;

                        Sounds[soundName].PlayInstance();
                    }
                });
        }

        public static TextureInfo GetTextureFromName(string textureName)
        {
            if (Textures.Keys.Contains(textureName))
                return Textures[textureName];

            return default(TextureInfo);
        }

        public static AnimatedSprite GetAnimationFromName(string animationName)
        {
            if (AnimatedEntities.Keys.Contains(animationName))
                return AnimatedEntities[animationName];

            return default(AnimatedSprite);
        }

        private static void LoadThemeCache()
        {
            if (!File.Exists(ThemeCacheLocation))
                CreateThemeCache();

            var xElement = XElement.Load(ThemeCacheLocation);
            var themes = xElement.Elements("Theme");

            foreach (var element in themes)
            {
                var relativePath = @"Content";

                if (element.Attribute("location") != null)
                    relativePath += element.Attribute("location").Value;

                var themeFiles = Directory.GetFiles(relativePath, "*" + ThemeExtension, SearchOption.TopDirectoryOnly);
                
                if (themeFiles.Length > 0)
                {
                    var file = themeFiles.First();
                    var themeInfo = GetThemeInfo(file);

                    themeInfo.Location = file;

                    if (!string.IsNullOrEmpty(themeInfo.Name))
                        Themes.Add(themeInfo);
                }
            }
        }

        private static void CreateThemeCache()
        {
            var relativePath = "Content";

            var directory = Path.GetDirectoryName(ThemeCacheLocation);
            var files = Directory.GetFiles(directory, "*" + ThemeExtension, SearchOption.AllDirectories);

            var themeInfoCollection = new List<ThemeInfo>();

            foreach (var file in files)
            {
                var info = GetThemeInfo(file);

                if (!string.IsNullOrEmpty(info.Name))
                {
                    var relative = file.Replace(relativePath, "");
                    relative = Path.GetDirectoryName(relative);
                    info.Location = relative;
                }

                if (!string.IsNullOrEmpty(info.Location))
                    themeInfoCollection.Add(info);
            }

            var xElement = new XElement("Themes", new XAttribute("ver", "1.0"));

            foreach (var item in themeInfoCollection)
            {
                var themeElement = new XElement("Theme", new XAttribute("name", item.Name));

                if (!string.IsNullOrEmpty(item.Alias))
                    themeElement.Add(new XAttribute("alias", item.Alias));
                
                themeElement.Add(new XAttribute("location", item.Location));
                xElement.Add(themeElement);
            }

            using (var fStream = new FileStream(ThemeCacheLocation, FileMode.Create, FileAccess.ReadWrite))
            {
                using (var sWriter = new StreamWriter(fStream))
                    sWriter.Write(xElement.ToString(SaveOptions.None));
            }
        }

        private static void LoadThemeFromInfo(ThemeInfo info)
        {
            var name = info.Name;
            var alias = info.Alias;

            if (!info.Location.EndsWith(ThemeExtension))
            {
                var files = Directory.GetFiles(info.Location, "*" + ThemeExtension, SearchOption.TopDirectoryOnly);

                if (files.Length == 0)
                    throw new FileNotFoundException(string.Format("Cannot file theme file in the directory: {0}", Path.GetFileNameWithoutExtension(info.Location)));

                foreach (var fileName in files)
                {
                    var getInfo = GetThemeInfo(fileName);

                    if (info.Equals(getInfo))
                    {
                        LoadThemeContent(fileName);
                        break;
                    }
                }
            }
            else LoadThemeContent(info.Location);

            Name = info.Name;
            Alias = info.Alias;
        }

        private static void LoadThemeContent(string fileLocation)
        {
            var theme = XElement.Load(fileLocation);
            var contentLocations = theme.Elements("ContentLocations");

            foreach (var element in contentLocations)
            {
                var contentCollection = element.Elements("ContentCollection");

                foreach (var contentElement in contentCollection)
                {
                    if (contentElement.Attribute("type") != null)
                    {
                        var elements = contentElement.Elements("content");
                        var contentType = (ContentType)Enum.Parse(typeof(ContentType), contentElement.Attribute("type").Value, true);

                        switch(contentType)
                        {
                            #region Graphics

                            case ContentType.Graphics:
                                
                                foreach (var graphic in elements)
                                {
                                    if (graphic.Attribute("name") != null && graphic.Attribute("location") != null)
                                    {
                                        var name = graphic.Attribute("name").Value;
                                        var location = Path.GetDirectoryName(fileLocation) + graphic.Attribute("location").Value;

                                        var textureInfo = new TextureInfo();
                                        var propertyElement = graphic.Element("properties");

                                        if (propertyElement != null)
                                        {
                                            var topCoordinate = 0.0f;
                                            var leftCoordinate = 0.0f;
                                            var rightCoordinate = 0.0f;
                                            var bottomCoordinate = 0.0f;

                                            if (propertyElement.Attribute("topCoordinate") != null)
                                                topCoordinate = float.Parse(propertyElement.Attribute("topCoordinate").Value);

                                            if (propertyElement.Attribute("leftCoordinate") != null)
                                                leftCoordinate = float.Parse(propertyElement.Attribute("leftCoordinate").Value);

                                            if (propertyElement.Attribute("bottomCoordinate") != null)
                                                bottomCoordinate = float.Parse(propertyElement.Attribute("bottomCoordinate").Value);

                                            if (propertyElement.Attribute("rightCoordinate") != null)
                                                rightCoordinate = float.Parse(propertyElement.Attribute("rightCoordinate").Value);

                                            textureInfo.TopTextureCoordinate = topCoordinate;
                                            textureInfo.LeftTextureCoordinate = leftCoordinate;
                                            textureInfo.BottomTextureCoordinate = bottomCoordinate;
                                            textureInfo.RightTextureCoordinate = rightCoordinate;
                                        }

                                        textureInfo.Texture = ContentManager.LoadTexture(location);

                                        if (!Textures.Keys.Contains(name))
                                            Textures.Add(name, textureInfo);
                                    }
                                }

                                break;

                            #endregion

                            #region Sound

                            case ContentType.Sound:
                                
                                foreach (var sound in elements)
                                {
                                    if (sound.Attribute("name") != null && sound.Attribute("location") != null)
                                    {
                                        var name = sound.Attribute("name").Value;
                                        var location = Path.GetDirectoryName(fileLocation) + sound.Attribute("location").Value;
                                        var soundFile = ContentManager.LoadSound(location);

                                        if (!Sounds.ContainsKey(name) && soundFile != null)
                                            Sounds.Add(name, soundFile);
                                    }
                                }

                                break;

                            #endregion

                            #region Animated Sprite

                            case ContentType.AnimatedSprite:

                                foreach (var animation in elements)
                                {
                                    if (animation.Attribute("name") != null && animation.Attribute("location") != null)
                                    {
                                        var name = animation.Attribute("name").Value;
                                        var location = Path.GetDirectoryName(fileLocation) + animation.Attribute("location").Value;

                                        var propertyElement = animation.Element("properties");

                                        if (propertyElement != null)
                                        {                                            
                                            var propertyFrames = propertyElement.Elements("frame");

                                            if (propertyFrames != null)
                                            {
                                                var animatedSprite = new AnimatedSprite();                                               
                                                
                                                if (propertyElement.Attribute("framerate") != null)
                                                    animatedSprite.FrameRate = float.Parse(propertyElement.Attribute("framerate").Value);

                                                foreach (var frameElement in propertyFrames)
                                                {
                                                    var frame = new TextureInfo();

                                                    if (frameElement.Attribute("topCoordinate") != null)
                                                        frame.TopTextureCoordinate = float.Parse(frameElement.Attribute("topCoordinate").Value);

                                                    if (frameElement.Attribute("leftCoordinate") != null)
                                                        frame.LeftTextureCoordinate = float.Parse(frameElement.Attribute("leftCoordinate").Value);

                                                    if (frameElement.Attribute("bottomCoordinate") != null)
                                                        frame.BottomTextureCoordinate = float.Parse(frameElement.Attribute("bottomCoordinate").Value);

                                                    if (frameElement.Attribute("rightCoordinate") != null)
                                                        frame.RightTextureCoordinate = float.Parse(frameElement.Attribute("rightCoordinate").Value);

                                                    frame.Texture = ContentManager.LoadTexture(location);

                                                    if (!Textures.Keys.Contains(name))
                                                        Textures.Add(name, frame);

                                                    animatedSprite.Frames.Add(frame);
                                                }

                                                if (!AnimatedEntities.Keys.Contains(name))
                                                    AnimatedEntities.Add(name, animatedSprite);
                                            }
                                        }
                                    }
                                }

                                break;

                            #endregion
                        }
                    }
                }
            }

        }

        private bool VerifyThemeLocation(string fileLocation)
        {

            return File.Exists(fileLocation + ThemeExtension);
        }
        
        private static ThemeInfo GetThemeInfo(string fileLocation)
        {
            if (!File.Exists(fileLocation))
                throw new FileNotFoundException(string.Format("{0} does not exist in the directory: {1}", Path.GetFileNameWithoutExtension(fileLocation), Path.GetDirectoryName(fileLocation)));

            var themeInfo = new ThemeInfo();

            var xmlSettings = new XmlReaderSettings() 
            { 
                CheckCharacters = false, 
                ConformanceLevel = ConformanceLevel.Fragment, 
                IgnoreWhitespace = true, 
                CloseInput = true 
            };
            
            using (var xReader = XmlReader.Create(fileLocation, xmlSettings))
            {
                var xElement = XElement.Load(xReader);

                if (xElement.Name == "Theme")
                {
                    if (xElement.Attribute("name") != null)
                        themeInfo.Name = xElement.Attribute("name").Value;

                    if (xElement.Attribute("alias") != null)
                        themeInfo.Alias = xElement.Attribute("alias").Value;

                    if (xElement.Attribute("supports") != null)
                    {
                        var flags = xElement.Attribute("supports").Value;
                        flags = flags.Replace("|", ",");

                        themeInfo.SupportLevel = (GraphicsSupportLevel)Enum.Parse(typeof(GraphicsSupportLevel), flags);
                    }

                    if (xElement.Attribute("version") != null)
                        themeInfo.Version = float.Parse(xElement.Attribute("version").Value);

                    else themeInfo.Version = 1.0f;
                }
            }

            return themeInfo;
        }

        private static ContentManager ContentManager;

        private static object SoundSync;

        private static Dictionary<string, SoundFX> Sounds;
        private static Dictionary<string, TextureInfo> Textures;
        private static Dictionary<string, AnimatedSprite> AnimatedEntities;
        // private static Dictionary<string, Font> Fonts;
        // private static Dictionary<string, Video> Videos;
    }
}
