using System;
using System.Collections.Generic;

namespace Generators.MAVLinkDrone
{
    public class PyroEvent
    {
        public readonly TimeSpan startTime; //this is actually pre-adjusted when its handed to us for the prefire time
        public readonly int pyroIndex;
        public readonly int pitch;
        public readonly int yaw;
        public readonly int roll;
        public readonly TimeSpan duration;
        public readonly TimeSpan endTime;
        public const int millisecondsPerFiring = 1000;

        //blank pyro event
        public PyroEvent()
        {
            pyroIndex = 0;
            pitch = 0;
            yaw = 0;
            roll = 0;
        }

        public PyroEvent(ref Queue<byte> data)
        {
            startTime = TimeSpan.FromMilliseconds(ShowFile.GetVarInt(ref data));
            pyroIndex = ShowFile.GetVarInt(ref data);

            //setup the other read only values
            duration = TimeSpan.FromMilliseconds(millisecondsPerFiring);
            endTime = startTime + duration;

            pitch = ShowFile.GetVarInt(ref data);
            yaw = ShowFile.GetVarInt(ref data);
            roll = ShowFile.GetVarInt(ref data);
        }

        //inside check
        public bool InsideEvent(TimeSpan time)
        {
            //check if the time is inside the event
            return time >= startTime && time < endTime;
        }
    }
}
