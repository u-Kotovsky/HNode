using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TextureWriter;

public class FuralitySomna : IDMXSerializer
{
    const int blockSize = 16; // 10x10 pixels per channel block
    const int blocksPerCol = 13; // channels per column
    public Dictionary<int, ColorChannel> mergedChannels = new Dictionary<int, ColorChannel>();
    
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
