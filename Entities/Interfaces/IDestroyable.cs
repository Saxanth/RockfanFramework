using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rockfan.Framework.Entities.Interfaces
{
    public interface IDestroyable : IEntity
    {
        void Destroy();
    }
}
