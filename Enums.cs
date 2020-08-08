using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rockfan.Framework
{
    [Flags()]
    public enum SortType : byte
    {
        None = 0x01,
        AwayFromCamera = 0x02,
        ReverseSort = 0x04
    }

    [Flags()]
    public enum GraphicsSupportLevel : byte
    {
        Low     = 0x01,
        Medium  = 0x02,
        High    = 0x04,
        Extreme = 0x08
    }

    public enum ContentType : byte
    {
        Font = 0x01,
        Sound = 0x02,
        Graphics = 0x03,
        Model = 0x04,
        Video = 0x05,
        AnimatedSprite = 0x06
    }
    
    public enum AudioEventType
    {
        Completed
    }

    public enum PlayerType
    {
        Human,
        Computer,
        RemotePlayer
    }

    public enum PlayerIndex
    {
        One,
        Two,
        Three,
        Four
    }

}

