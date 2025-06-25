using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TextureWriter;

public class FuralitySomna : IDMXSerializer
{
    static List<int> mergedChannels = new List<int>()
    {
        0, 1, 2

    };
    public void MapChannel(ref Color32[] pixels, byte channelValue, int channel, int textureWidth, int textureHeight)
    {
        //convert the channel to x y
        const int blockSize = 16; // 10x10 pixels per channel block
        const int blocksPerCol = 13; // channels per column

        int cumulativeOFfset = 0;
        //we want to build up to how many pixels we have already done based on our channel count
        foreach (int channelI in mergedChannels)
        {
            if (channelI < channel)
            {
                cumulativeOFfset--;
            }
        }

        int x = ((channel - cumulativeOFfset) / blocksPerCol) * blockSize;
        int y = ((channel - cumulativeOFfset) % blocksPerCol) * blockSize;

        if (mergedChannels.Contains(channel))
        {
            ColorChannel channelType = (ColorChannel)mergedChannels.IndexOf(channel % 3);
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
    }
}
