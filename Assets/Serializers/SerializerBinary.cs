using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Binary : IDMXSerializer
{
    public static void MapChannel(ref Color32[] pixels, byte channelValue, int channel, int textureWidth, int textureHeight)
    {
        //convert the channel to x y
        const int blockSize = 4; // 10x10 pixels per channel block
        const int blocksPerCol = 52; // channels per column

        //split the value into 8 bits
        var bits = new BitArray(new byte[] { channelValue });

        for (int i = 0; i < bits.Length; i++)
        {
            int newChannel = (channel * 8) + i;
            int x = (newChannel / blocksPerCol) * blockSize;
            int y = (newChannel % blocksPerCol) * blockSize;
            if (x >= textureWidth || y >= textureHeight)
            {
                continue; // Skip if the calculated pixel is out of bounds
            }
            //convert the x y to pixel index
            //return 4x4 area
            var color = new Color32(
                (byte)(bits[i] ? 255 : 0),
                (byte)(bits[i] ? 255 : 0),
                (byte)(bits[i] ? 255 : 0),
                255
            );
            TextureWriter.MakeColorBlock(ref pixels, x, y, color, blockSize);
        }
    }
}
