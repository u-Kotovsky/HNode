using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Strobe : IDMXGenerator
{
    public int channel = 0;
    public byte valueOn = 255;
    public byte valueOff = 0;
    public float frequency = 1.0f; //in Hz
    public void GenerateDMX(ref List<byte> dmxData)
    {
        dmxData.EnsureCapacity(channel);

        //use system time to determine if we are on or off
        var time = DateTime.Now.Millisecond;

        //now determine if we are on or off
        float period = 1000.0f / frequency; //in ms

        if (time % period < period / 2)
        {
            dmxData[channel] = valueOn;
        }
        else
        {
            dmxData[channel] = valueOff;
        }
    }
}
