using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Text : IDMXGenerator
{
    public string text = "Hello World";
    public int channelStart = 0;
    public void GenerateDMX(ref List<byte> dmxData)
    {
        //convert text to DMX data
        byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(text);

        if (dmxData.Count < channelStart + textBytes.Length)
        {
            //expand it by the proper amount
            int newSize = channelStart + textBytes.Length;
            int difference = newSize - dmxData.Count;
            dmxData.AddRange(new byte[difference]);
        }

        //we need to write to the dmx data list directly
        for (int i = channelStart; i < channelStart + textBytes.Length; i++)
        {
            dmxData[i] = textBytes[i - channelStart];
        }
    }
}
