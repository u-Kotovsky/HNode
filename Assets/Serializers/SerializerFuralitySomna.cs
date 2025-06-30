using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TextureWriter;

public class FuralitySomna : IDMXSerializer
{
    const int blockSize = 16; // 10x10 pixels per channel block
    const int blocksPerCol = 13; // channels per column
    Dictionary<int, ColorChannel> mergedChannels = new Dictionary<int, ColorChannel>()
    {
        {7, ColorChannel.Red},
        {8, ColorChannel.Green},
        {9, ColorChannel.Blue},
        {7 + 13, ColorChannel.Red},
        {8 + 13, ColorChannel.Green},
        {9 + 13, ColorChannel.Blue},
        {7 + (13 * 2), ColorChannel.Red},
        {8 + (13 * 2), ColorChannel.Green},
        {9 + (13 * 2), ColorChannel.Blue},
    };
    int cumulativeOFfset = 0;

    public void InitFrame() { cumulativeOFfset = 0; }
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
                255
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
}
