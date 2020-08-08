using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rockfan.Framework
{
    public sealed class ThemeInfo
    {
        public string Name { get; internal set; }
        public string Alias { get; internal set; }

        public float Version { get; internal set; }
        public GraphicsSupportLevel SupportLevel { get; internal set; }

        public string Location { get; internal set; }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(ThemeInfo))
                return false;

            var comparer = (ThemeInfo)obj;

            return comparer.Alias == this.Alias &&
                comparer.Location == this.Location &&
                comparer.Name == this.Name &&
                comparer.SupportLevel == this.SupportLevel &&
                comparer.Version == this.Version;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }

    }
}
