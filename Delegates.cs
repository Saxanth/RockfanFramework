using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rockfan.Framework
{
    public delegate void ProgressChanged(object sender, float progress);
    public delegate void AudioEventHandler(object sender, AudioEventType eventType);
}
