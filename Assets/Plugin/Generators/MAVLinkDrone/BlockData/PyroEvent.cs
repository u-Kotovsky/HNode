using System;
using System.Collections.Generic;

namespace Generators.MAVLinkDrone
{
    public class PyroEvent
    {
        public readonly TimeSpan startTime;
        public readonly int pyroIndex;
        public readonly TimeSpan duration;
        public readonly TimeSpan endTime;
        public const int millisecondsPerFiring = 1000;

        public PyroEvent(ref Queue<byte> data)
        {
            startTime = TimeSpan.FromMilliseconds(ShowFile.GetVarInt(ref data));
            pyroIndex = ShowFile.GetVarInt(ref data);

            //setup the other read only values
            duration = TimeSpan.FromMilliseconds(millisecondsPerFiring);
            endTime = startTime + duration;
        }

        //inside check
        public bool InsideEvent(TimeSpan time)
        {
            //check if the time is inside the event
            return time >= startTime && time < endTime;
        }
    }
}
