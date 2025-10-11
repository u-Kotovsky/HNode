using System;
using System.Collections.Generic;
using UnityEngine;

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
        public const int minMillisecondsPerFiring = 300;

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
            duration = TimeSpan.FromMilliseconds(ShowFile.GetVarInt(ref data));
            duration = TimeSpan.FromMilliseconds(Math.Max(duration.TotalMilliseconds, minMillisecondsPerFiring));

            //setup the other read only values
            endTime = startTime + duration;

            pyroIndex = ShowFile.GetVarInt(ref data);

            //the sign of the angles is stored in the next byte
            byte signs = data.Dequeue();
            int pitchSign = (signs & 0b00000100) == 0 ? 1 : -1;
            int yawSign = (signs & 0b00000010) == 0 ? 1 : -1;
            int rollSign = (signs & 0b00000001) == 0 ? 1 : -1;

            pitch = ShowFile.GetVarInt(ref data) * pitchSign;
            yaw = ShowFile.GetVarInt(ref data) * yawSign;
            roll = ShowFile.GetVarInt(ref data) * rollSign;
            Debug.Log($"Pyro event: Start {startTime.TotalMilliseconds}ms  Index {pyroIndex}  Pitch {pitch}  Yaw {yaw}  Roll {roll}");
        }

        //inside check
        public bool InsideEvent(TimeSpan time)
        {
            //check if the time is inside the event
            return time >= startTime && time < endTime;
        }
    }
}
