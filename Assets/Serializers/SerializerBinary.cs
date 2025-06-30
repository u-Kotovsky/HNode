using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Binary : IDMXSerializer
{
    const int blockSize = 4; // 10x10 pixels per channel block
    const int blocksPerCol = 52; // channels per column
    public void InitFrame() { }

    public void SerializeChannel(ref Color32[] pixels, byte channelValue, int channel, int textureWidth, int textureHeight)
    {
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

    public void DeserializeChannel(Texture2D tex, ref byte channelValue, int channel, int textureWidth, int textureHeight)
    {
        var bits = new BitArray(8);
        for (int i = 0; i < bits.Length; i++)
        {
            int newChannel = (channel * 8) + i;
            int x = (newChannel / blocksPerCol) * blockSize;
            int y = (newChannel % blocksPerCol) * blockSize;
            //add on a offset
            x += 1;
            y += 1;
            if (x >= textureWidth || y >= textureHeight)
            {
                continue; // Skip if the calculated pixel is out of bounds
            }
            // Read the 4x4 area and combine it into a single byte
            bits[i] = TextureReader.GetColor(tex, x, y).r > 0.5f;
        }
        // Convert the BitArray back to a byte
        channelValue = ConvertToByte(bits);
    }

    byte ConvertToByte(BitArray bits)
    {
        if (bits.Count != 8)
        {
            throw new ArgumentException("bits");
        }
        byte[] bytes = new byte[1];
        bits.CopyTo(bytes, 0);
        return bytes[0];
    }
}
