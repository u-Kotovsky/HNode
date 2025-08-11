using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using static TextureWriter;

public class FuralitySomna : IDMXSerializer
{
    const int blockSize = 16; // 10x10 pixels per channel block
    const int blocksPerCol = 13; // channels per column
    public Dictionary<DMXChannel, ColorChannel> mergedChannels = new Dictionary<DMXChannel, ColorChannel>();

    int cumulativeOFfset = 0;

    public void Construct() { }
    public void Deconstruct() { }

    public void InitFrame()
    {
        cumulativeOFfset = 0;
    }
    public void CompleteFrame(ref Color32[] pixels, ref List<byte> channelValues, int textureWidth, int textureHeight) { }
    public void SerializeChannel(ref Color32[] pixels, byte channelValue, int channel, int textureWidth, int textureHeight)
    {
        int x = ((channel - cumulativeOFfset) / blocksPerCol) * blockSize;
        int y = ((channel - cumulativeOFfset) % blocksPerCol) * blockSize;

        if (mergedChannels.ContainsKey(channel))
        {
            ColorChannel channelType = mergedChannels[channel];
            TextureWriter.MixColorBlock(ref pixels, x, y, channelValue, channelType, blockSize);
        }
        else
        {
            var color = new Color32(
                channelValue,
                channelValue,
                channelValue,
                Util.GetBlockAlpha(channelValue)
            );
            TextureWriter.MakeColorBlock(ref pixels, x, y, color, blockSize);
        }

        if (mergedChannels.ContainsKey(channel))
        {
            //if its blue, dont increment the offset
            if (mergedChannels[channel] == ColorChannel.Blue)
            {
                return;
            }
            cumulativeOFfset++;
        }
    }

    public void DeserializeChannel(Texture2D tex, ref byte channelValue, int channel, int textureWidth, int textureHeight) => throw new NotImplementedException();

    public void ConstructUserInterface(RectTransform rect)
    {

    }

    public void DeconstructUserInterface()
    {

    }

    public void UpdateUserInterface()
    {

    }
}
