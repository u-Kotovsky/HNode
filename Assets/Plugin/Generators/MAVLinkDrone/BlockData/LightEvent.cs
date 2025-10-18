using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Generators.MAVLinkDrone
{
    public class LightEvent
    {
        public readonly TimeSpan duration;
        public readonly Color32 color;
        public Color32 previousEventColor = new Color32(0, 0, 0, 255); //default to black as the previous color
        public bool setsColor => opcode switch
        {
            Opcode.SET_COLOR or
            Opcode.SET_COLOR_FROM_CHANNELS or
            Opcode.SET_GRAY or
            Opcode.SET_BLACK or
            Opcode.SET_WHITE or
            Opcode.FADE_TO_COLOR or
            Opcode.FADE_TO_COLOR_FROM_CHANNELS or
            Opcode.FADE_TO_GRAY or
            Opcode.FADE_TO_BLACK or
            Opcode.FADE_TO_WHITE => true,
            _ => false,
        };
        public readonly Opcode opcode;
        public byte? counter = null;
        public int? address = null;
        public TimeSpan startTime = new TimeSpan(-50);
        public TimeSpan endTime = new TimeSpan(-1);

        public bool IsFade => opcode switch
        {
            Opcode.FADE_TO_COLOR or
            Opcode.FADE_TO_COLOR_FROM_CHANNELS or
            Opcode.FADE_TO_GRAY or
            Opcode.FADE_TO_BLACK or
            Opcode.FADE_TO_WHITE => true,
            _ => false,
        };

        public LightEvent(ref Queue<byte> data)
        {
            //pop the first byte to get the opcode
            var opcoderaw = data.DequeueChunk(1).First();
            opcode = (Opcode)opcoderaw;
            //Debug.Log($"Opcode: {opcode}");

            //assign duration as 0
            duration = TimeSpan.Zero;

            byte tempByte;

            switch (opcode)
            {
                case Opcode.END:
                case Opcode.NOP:
                case Opcode.LOOP_END:
                case Opcode.RESET_CLOCK:
                case Opcode.UNUSED_1:
                    break;
                case Opcode.SLEEP:
                case Opcode.WAIT_UNTIL: //this one isnt technically correct but its not used anymore anyway so whatever
                    duration = GetDuration(ref data);
                    break;
                case Opcode.SET_COLOR:
                case Opcode.SET_COLOR_FROM_CHANNELS: //99% sure this works the same way
                    color = new Color32(
                        data.DequeueChunk(1).First(),
                        data.DequeueChunk(1).First(),
                        data.DequeueChunk(1).First(),
                        255
                    );
                    duration = GetDuration(ref data);
                    break;
                case Opcode.SET_GRAY:
                    tempByte = data.DequeueChunk(1).First();
                    color = new Color32(
                        tempByte,
                        tempByte,
                        tempByte,
                        255
                    );
                    duration = GetDuration(ref data);
                    break;
                case Opcode.SET_BLACK:
                    color = new Color32(
                        0,
                        0,
                        0,
                        255
                    );
                    duration = GetDuration(ref data);
                    break;
                case Opcode.SET_WHITE:
                    color = new Color32(
                        255,
                        255,
                        255,
                        255
                    );
                    duration = GetDuration(ref data);
                    break;
                case Opcode.FADE_TO_COLOR:
                case Opcode.FADE_TO_COLOR_FROM_CHANNELS:
                    color = new Color32(
                        data.DequeueChunk(1).First(),
                        data.DequeueChunk(1).First(),
                        data.DequeueChunk(1).First(),
                        255
                    );
                    duration = GetDuration(ref data);
                    break;
                case Opcode.FADE_TO_GRAY:
                    tempByte = data.DequeueChunk(1).First();
                    color = new Color32(
                        tempByte,
                        tempByte,
                        tempByte,
                        255
                    );
                    duration = GetDuration(ref data);
                    break;
                case Opcode.FADE_TO_BLACK:
                    color = new Color32(
                        0,
                        0,
                        0,
                        255
                    );
                    duration = GetDuration(ref data);
                    break;
                case Opcode.FADE_TO_WHITE:
                    color = new Color32(
                        255,
                        255,
                        255,
                        255
                    );
                    duration = GetDuration(ref data);
                    break;
                case Opcode.LOOP_BEGIN:
                    counter = data.DequeueChunk(1).First();
                    break;
                case Opcode.JUMP:
                    address = ShowFile.GetVarInt(ref data);
                    break;
                //ignore a setpyro opcode
                case Opcode.SET_PYRO:
                case Opcode.SET_PYRO_ALL:
                    tempByte = data.DequeueChunk(1).First(); //just ignore the argument byte
                    break;
                default:
                    throw new NotImplementedException($"Unhandled opcode: {opcode} OR {opcoderaw.ToString("X2")}, trace of the next few bytes: {string.Join(" ", data.Take(10).Select(b => b.ToString("X2")))}");
            }

            //Debug.Log($"Light Event: Opcode: {opcode}, Duration: {duration}, Color: {color}, Counter: {counter}, Address: {address}");
        }

        public bool InsideEvent(TimeSpan time)
        {
            //check if the time is inside the event
            return time >= startTime && time < endTime;
        }

        private static TimeSpan GetDuration(ref Queue<byte> data)
        {
            return TimeSpan.FromMilliseconds(ShowFile.GetVarInt(ref data) * 20);
        }

        //https://github.com/skybrush-io/libskybrush/blob/9ecf48d6fcf258c5be77bf31d88a91241b1a5700/src/lights/commands.h#L39
        //https://github.com/skybrush-io/libskybrush/blob/9ecf48d6fcf258c5be77bf31d88a91241b1a5700/src/lights/commands.cpp#L104
        public enum Opcode
        {
            END = 0,
            NOP = 1,
            SLEEP = 2,
            WAIT_UNTIL = 3, //apparently not used anymore
            SET_COLOR = 4,
            SET_GRAY = 5,
            SET_BLACK = 6,
            SET_WHITE = 7,
            FADE_TO_COLOR = 8,
            FADE_TO_GRAY = 9,
            FADE_TO_BLACK = 10,
            FADE_TO_WHITE = 11,
            LOOP_BEGIN = 12, //ignore, shouldnt be used anymore
            LOOP_END = 13, //ignore, shouldnt be used anymore
            RESET_CLOCK = 14, //ignore, shouldnt be used anymore
            UNUSED_1 = 15, //ignore, shouldnt be used anymore
            SET_COLOR_FROM_CHANNELS = 16, //ignore, shouldnt be used anymore
            FADE_TO_COLOR_FROM_CHANNELS = 17, //ignore, shouldnt be used anymore
            JUMP = 18, //ignore, shouldnt be used anymore
            TRIGGERED_JUMP = 19, //ignored for now
            SET_PYRO = 20, //ignored for now TODO: Implement for pyro control
            SET_PYRO_ALL = 21, //ignored for now TODO: Implement for pyro control
            NUMBER_OF_COMMANDS, //automatic assignment to match the number of commands
        }
    }
}
