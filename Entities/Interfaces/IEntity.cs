using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rockfan.Framework.Entities.Interfaces
{
    public interface IEntity
    {
        string Name { get; }
        float TimeCreated { get; }
    }
}
