using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Generators.MAVLinkDrone
{
    public class ShowFile
    {
        //programs
        public List<LightEvent> LightProgram = new();
        public int LightProgramPointer = 0;
        public List<Trajectory> TrajectoryProgram = new();
        public int TrajectoryProgramPointer = 0;
        public List<PyroEvent> PyroProgram = new();
        //public int PyroProgramPointer = 0;
        const int PointerLookahead = 5;

        public DateTime showStartTime = DateTime.UtcNow + TimeSpan.FromDays(2); //assume way into the future

        public ShowFile(List<byte> rawData)
        {
            //make a copy to operate on
            Queue<byte> fileData = new Queue<byte>(rawData);

            //file header is the first 10 bytes, pull that out of the buffer
            List<byte> header = fileData.DequeueChunk(10).ToList();

            //next we should have a block header that is 3 bytes long.
            while (fileData.Count > 0)
            {
                ExtractBlock(fileData);
            }
        }

        private void ExtractBlock(Queue<byte> fileData)
        {
            //the first byte is the block type
            //the next 2 bytes are the block size
            BlockType blockType = (BlockType)fileData.DequeueChunk(1).First();
            int blockSize = BitConverter.ToUInt16(fileData.DequeueChunk(2).ToArray(), 0);
            //Debug.Log($"Block Type: {blockType}, Block Size: {blockSize}");
            Queue<byte> blockData = new(fileData.DequeueChunk(blockSize));
            //Debug.Log(blockType);
            //Debug.Log(blockSize);
            //Debug.Log(blockData);

            switch (blockType)
            {
                case BlockType.LIGHT_PROGRAM:
                    while (blockData.Count > 0)
                    {
                        LightProgram.Add(new LightEvent(ref blockData));
                    }

                    /* //debug, total up all different events used
                    Dictionary<LightEvent.Opcode, int> eventTypes = new();
                    foreach (var lightEvent in LightProgram)
                    {
                        if (eventTypes.ContainsKey(lightEvent.opcode))
                        {
                            eventTypes[lightEvent.opcode]++;
                        }
                        else
                        {
                            eventTypes[lightEvent.opcode] = 1;
                        }
                    }

                    //print it out
                    Debug.Log("Light Event Types Used:");
                    foreach (var kvp in eventTypes)
                    {
                        Debug.Log($"{kvp.Key}: {kvp.Value}");
                    } */

                    //compute the start and end time for all of the events
                    if (LightProgram.Count > 0)
                    {
                        //set the start time of the first event
                        LightProgram[0].startTime = TimeSpan.Zero;
                        for (int i = 1; i < LightProgram.Count; i++)
                        {
                            //setup the end time of the last light event
                            LightProgram[i - 1].endTime = LightProgram[i - 1].startTime + LightProgram[i - 1].duration;
                            LightProgram[i].startTime = LightProgram[i - 1].endTime;
                        }

                        //setup the last end time
                        LightProgram.Last().endTime = LightProgram.Last().startTime + LightProgram.Last().duration;

                        //setup the cached previous event value
                        //cache the previous color and the start time
                        Color32 lastColor = Color.black;
                        foreach (var ev in LightProgram)
                        {
                            ev.previousEventColor = lastColor; //set the previous event color to the last color

                            if (ev.setsColor) //only do it if this event defines a color. This will make sure that the color is the latest event WITH a color define
                            {
                                lastColor = ev.color; //if no color is set, use black
                            }
                        }

                        //reset the program counter
                        LightProgramPointer = 0;
                    }

                    //debug print start and end time of events
                    /* foreach (var lightProgram in LightProgram)
                    {
                        Debug.Log($"Light Event: Opcode: {lightProgram.opcode}, Start Time: {lightProgram.startTime}, End Time: {lightProgram.endTime}, Duration: {lightProgram.duration}, Color: {lightProgram.color}, Counter: {lightProgram.counter}, Address: {lightProgram.address}");
                    } */

                    /* //print out the ENTIRE program as a formatted string
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Full Light Program:");
                    foreach (var eve in LightProgram)
                    {
                        sb.AppendLine($@"{eve.opcode}");
                        sb.AppendLine($"    Starts: {eve.startTime}, Ends: {eve.endTime}");
                        sb.AppendLine($"    Duration: {eve.duration}");
                        sb.AppendLine($"    Counter: {eve.counter}");
                        sb.AppendLine($"    Sets Color: {eve.setsColor} Is Fade: {eve.IsFade} Previous Color: {eve.previousEventColor}");
                    }
                    //print it
                    Debug.Log(sb.ToString()); */
                    break;
                case BlockType.TRAJECTORY:
                    //if the initial size is only 1, then this is an empty trajectory block
                    if (blockData.Count == 1)
                    {
                        Debug.Log("Empty Trajectory Block");
                        //set a blank trajectory
                        TrajectoryProgram.Add(new Trajectory());
                        break;
                    }

                    //decode the scale/flags, and the start xyz
                    //flags is the first byte
                    byte flags = blockData.DequeueChunk(1).First();
                    //MSB is unused, scale is the remaining 7 bits
                    // 0x7f is 01111111
                    byte scale = (byte)(flags & 0x7F);
                    Vector3 startPos = Trajectory.DecodeStartSpatialCoordinates(ref blockData, scale);
                    float startYaw = Trajectory.DecodeAngleCoordinate(ref blockData);

                    //print this info
                    //Debug.Log($"Scale: {scale}, Trajectory Start: {startPos}, Yaw: {startYaw}");

                    while (blockData.Count > 0)
                    {
                        TrajectoryProgram.Add(new Trajectory(ref blockData, startPos, startYaw, scale));
                        //next startpos is the trajectory we just added
                        startPos = TrajectoryProgram.Last().lastPosition;
                        startYaw = TrajectoryProgram.Last().yawControlPoints.Last();
                    }

                    //compute the start and end time for all of the events
                    if (TrajectoryProgram.Count > 0)
                    {
                        //set the start time of the first event
                        TrajectoryProgram[0].startTime = TimeSpan.Zero;
                        for (int i = 1; i < TrajectoryProgram.Count; i++)
                        {
                            //setup the end time of the last trajectory
                            TrajectoryProgram[i - 1].endTime = TrajectoryProgram[i - 1].startTime + TrajectoryProgram[i - 1].duration;
                            TrajectoryProgram[i].startTime = TrajectoryProgram[i - 1].endTime;
                        }

                        //setup the last end time
                        TrajectoryProgram.Last().endTime = TrajectoryProgram.Last().startTime + TrajectoryProgram.Last().duration;

                        //reset the program counter
                        TrajectoryProgramPointer = 0;
                    }
                    break;
                case BlockType.EVENT_LIST:
                    //Debug.Log("Parsing Pyro Event List");
                    //TODO: This is a extremely not accurate section of this API, as to be accurate would need another license that isnt worth paying for
                    //TLDR, this will ONLY work with our custom version of the skybrush server code lol. Maybe find someone with a proper license in the future or rework this API
                    while (blockData.Count > 0)
                    {
                        //we ONLY expect pyro data at the moment, so there isnt anything identifying what a event is
                        //each pyro event has a varint millisecond trigger time, and a varint for the index selection
                        PyroProgram.Add(new PyroEvent(ref blockData));
                    }

                    /* //test print all the information
                    foreach (var pyro in PyroProgram)
                    {
                        Debug.Log($"Event at {pyro.eventTime}, Index {pyro.pyroIndex}");
                    } */
                    break;
                default:
                    Debug.LogWarning($"Unhandled block type: {blockType}");
                    break;
            }
        }

        public Vector3 GetPositionAtRealTime(DateTime time)
        {
            //convert the time to a timespan since the show start time
            TimeSpan elapsed = time - showStartTime;
            return GetPositionAtTime(elapsed);
        }

        public Vector3 GetPositionAtTime(TimeSpan time)
        {
            //if we are PAST the last one, just use that final value
            if (time > TrajectoryProgram.Last().endTime)
            {
                //Debug.Log($"Early exit at end of prog, {time}   {TrajectoryProgram.Last().endTime}");
                return TrajectoryProgram.Last().evaluate(1.0f);
            }

            //if we are before the first one, just use the first one
            if (time < TrajectoryProgram.First().startTime)
            {
                //Debug.Log($"Early exit at start of prog, {time}   {TrajectoryProgram.First().startTime}");
                return TrajectoryProgram.First().evaluate(0.0f);
            }

            //check if we are not in a light event now
            if (!TrajectoryProgram[TrajectoryProgramPointer].InsideEvent(time))
            {
                //look ahead at the next 5 and see if we are in one of them
                for (int i = TrajectoryProgramPointer + 1; i < TrajectoryProgram.Count && i < TrajectoryProgramPointer + PointerLookahead; i++)
                {
                    if (TrajectoryProgram[i].InsideEvent(time))
                    {
                        //we are in this light event, set the pointer to that
                        TrajectoryProgramPointer = i;
                        break;
                    }
                }
            }

            //Debug.Log(TrajectoryProgramPointer);
            Trajectory tevent = TrajectoryProgram[TrajectoryProgramPointer];

            //get the time inside the bezier curve
            float t = (float)((time - tevent.startTime) / tevent.duration);
            return tevent.evaluate(t);
        }

        public PyroEvent GetPyroAtRealTime(DateTime time)
        {
            //convert the time to a timespan since the show start time
            TimeSpan elapsed = time - showStartTime;
            return GetPyroAtTime(elapsed);
        }

        public PyroEvent GetPyroAtTime(TimeSpan time)
        {
            //if there is no pyro events, return 0
            if (PyroProgram.Count == 0)
            {
                return new PyroEvent();
            }

            //if this is before the first event, return 0
            if (time < PyroProgram.First().startTime)
            {
                return new PyroEvent();
            }

            //if its after, return black too
            if (time > PyroProgram.Last().endTime)
            {
                //Debug.Log($"Early exit at end of light prog, {time}   {LightProgram.Last().endTime}");
                return new PyroEvent();
            }

            //MORE EXPENSIVE WAY OF DOING IT BUT FUCK IT, PYRO DRONE COUNT IS SMALL
            //loop through ALL events, find which ones we are in, and then take the one that started MOST recently
            PyroEvent eve = new PyroEvent();
            foreach (var peve in PyroProgram)
            {
                //check if we are inside this event
                if (peve.InsideEvent(time))
                {
                    //is this the latest event?
                    if (peve.startTime > eve.startTime)
                    {
                        eve = peve;
                    }
                }
            }

            return eve;
        }

        public Color32 GetColorAtRealTime(DateTime time)
        {
            //convert the time to a timespan since the show start time
            TimeSpan elapsed = time - showStartTime;
            return GetColorAtTime(elapsed);
        }

        public Color32 GetColorAtTime(TimeSpan time)
        {
            //if this is before the first event, return black
            if (time < LightProgram.First().startTime)
            {
                //Debug.Log($"Early exit at start of light prog, {time}   {LightProgram.First().startTime}");
                return Color.black;
            }

            //if its after, return black too
            if (time > LightProgram.Last().endTime)
            {
                //Debug.Log($"Early exit at end of light prog, {time}   {LightProgram.Last().endTime}");
                return Color.black;
            }

            //check if we are not in a light event now
            if (!LightProgram[LightProgramPointer].InsideEvent(time))
            {
                //look ahead at the next 5 and see if we are in one of them
                for (int i = LightProgramPointer + 1; i < LightProgram.Count && i < LightProgramPointer + PointerLookahead; i++)
                {
                    if (LightProgram[i].InsideEvent(time))
                    {
                        //we are in this light event, set the pointer to that
                        LightProgramPointer = i;
                        break;
                    }
                }
            }

            LightEvent startevent = LightProgram[LightProgramPointer];

            //if this event is a fade
            if (startevent.IsFade)
            {
                //lerp between the last color and the current color
                if (startevent.setsColor)
                {
                    float t = Mathf.InverseLerp((float)startevent.startTime.TotalMilliseconds, (float)startevent.endTime.TotalMilliseconds, (float)time.TotalMilliseconds);
                    //if the color is set, use it
                    return Color32.Lerp(startevent.previousEventColor, startevent.color, t);
                }
            }

            //otherwise its a instantaneous set command
            if (startevent.setsColor)
            {
                //if the color is set, use it
                return startevent.color;
            }
            return startevent.previousEventColor;
        }

        enum BlockType : byte
        {
            TRAJECTORY = 1,
            LIGHT_PROGRAM = 2,
            COMMENT = 3,
            RTH_PLAN = 4,
            YAW_CONTROL = 5,
            EVENT_LIST = 6
        }

        public static int GetVarInt(ref Queue<byte> data)
        {
            //duration is encoded in varint format
            //MSB is 1 if the integer continues to the next byte, 0 if it is the last byte
            //duration is encoded as number of frames at 50 FPS, so each value is 20ms
            int val = 0;
            int shift = 0;
            byte b;
            do
            {
                b = data.DequeueChunk(1).First();
                val |= (b & 0x7F) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);

            return val;
        }
    }
}
