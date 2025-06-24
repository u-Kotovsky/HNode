using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorBinary : IDMXSerializer
{
    public void MapChannel(ref Color32[] pixels, byte channelValue, int channel, int textureWidth, int textureHeight)
    {
        //convert the channel to x y
        const int blockSize = 4; // 10x10 pixels per channel block
        const int blocksPerCol = 52; // channels per column

        //split the value into 8 bits
        var bits = new BitArray(new byte[] { channelValue });
        List<bool> bitsList = new List<bool>();
        for (int i = 0; i < bits.Length; i++)
        {
            bitsList.Add(bits[i]);
        }
        bitsList.Add(false); // Add a dummy bit to make it 9 bits, needed for easy interlacing

        for (int i = 0; i < bitsList.Count; i += 3)
        {
            int newChannel = (channel * 3) + i / 3; //3 because we interlace with color
            int x = (newChannel / blocksPerCol) * blockSize;
            int y = (newChannel % blocksPerCol) * blockSize;
            if (x >= textureWidth || y >= textureHeight)
            {
                continue; // Skip if the calculated pixel is out of bounds
            }
            //convert the x y to pixel index
            //return 4x4 area
            var color = new Color32(
                (byte)(bitsList[i] ? 255 : 0),
                (byte)(bitsList[i + 1] ? 255 : 0),
                (byte)(bitsList[i + 2] ? 255 : 0),
                255
            );
            TextureWriter.MakeColorBlock(ref pixels, x, y, color, blockSize);
        }
    }
}
