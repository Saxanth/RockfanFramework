using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rockfan.Framework;
using Rockfan.Framework.Audio;
using Rockfan.Framework.Entities;
using Rockfan.Framework.Entities.Interfaces;

namespace Rockfan.Framework.Audio
{
    public interface ISound : IUpdateableEntity, IDestroyable
    {
        #region Properties

        float Playlength { get; }

        bool IsPlaying { get; }

        bool IsLooped { get; }        

        float Volume { get; }

        float Playspeed { get; }

        float Playposition { get; }

        #endregion

        #region Methods

        void Play();

        void Pause();

        void Stop();

        #endregion
    }
}
