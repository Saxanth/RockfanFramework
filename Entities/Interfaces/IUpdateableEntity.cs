using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rockfan.Framework.Entities.Interfaces
{
    public interface IUpdateableEntity : IEntity
    {
        bool EnableUpdates { get; set; }

        void Update(float delta);
    }
}
