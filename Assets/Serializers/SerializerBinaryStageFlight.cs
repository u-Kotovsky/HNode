using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BinaryStageFlight : IDMXSerializer
{
    const int blockSize = 4; // 10x10 pixels per channel block
    const int channelsPerCol = 6;
    const int blocksPerCol = channelsPerCol * 8; // channels per column

    public void Construct() { }
    public void InitFrame() { }
    public void CompleteFrame(ref Color32[] pixels, ref List<byte> channelValues)
    {
        //figure out the lowest pixel it wouldve drawn before
        int startY = blocksPerCol * blockSize;

        //expand channelValues to a multiple of channelsPerCol
        int rounded = (int)(Math.Ceiling(channelValues.Count / (double)channelsPerCol) * channelsPerCol);
        channelValues.EnsureCapacity(rounded);

        //write out all the CRC information
        //the CRC is per collumn of data, so figure out the CRC for every channelsPerCol
        for (int i = 0; i < channelValues.Count; i += channelsPerCol)
        {
            byte[] values = channelValues.GetRange(i, channelsPerCol).ToArray();
            var crc = Crc4(values);

            //calculate the x
            int x = (i / channelsPerCol) * blockSize;
            //draw the 4 bits
            var bits = new BitArray(new byte[] { crc });
            for (int j = 0; j < /* bits.Length */ 4; j++)
            {
                int y = startY + j * blockSize;
                //convert the x y to pixel index
                //return 4x4 area
                var color = new Color32(
                    (byte)(bits[7 - j] ? 255 : 0),
                    (byte)(bits[7 - j] ? 255 : 0),
                    (byte)(bits[7 - j] ? 255 : 0),
                    Util.GetBlockAlpha(255) // Alpha should be forced on always
                );
                TextureWriter.MakeColorBlock(ref pixels, x, y, color, blockSize);
            }
        }
    }

    public void SerializeChannel(ref Color32[] pixels, byte channelValue, int channel, int textureWidth, int textureHeight)
    {
        //split the value into 8 bits
        var bits = new BitArray(new byte[] { channelValue });

        //sane endianness
        for (int i = 0; i < bits.Length; i++)
        {
            GetPositionData(channel, i, out int x, out int y);
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
                Util.GetBlockAlpha(channelValue)
            );
            TextureWriter.MakeColorBlock(ref pixels, x, y, color, blockSize);
        }
    }

    public void DeserializeChannel(Texture2D tex, ref byte channelValue, int channel, int textureWidth, int textureHeight)
    {
        //TODO: CRC Check for transcoding
        
        var bits = new BitArray(8);
        for (int i = 0; i < bits.Length; i++)
        {
            GetPositionData(channel, i, out int x, out int y);
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

    private static void GetPositionData(int channel, int i, out int x, out int y)
    {
        //int newChannel = (channel * 8) + i;
        //encode backwards, endiannes flip
        int newChannel = (channel * 8) + (7 - i);
        x = (newChannel / blocksPerCol) * blockSize;
        y = (newChannel % blocksPerCol) * blockSize;
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

    // CRC-4 (x⁴ + x + 1)
    public static byte Crc4(params byte[] data)
    {
        uint crc = 0;
        uint polynomial = 0x03;

        foreach (uint v in data)
        {
            for (int bit = 7; bit >= 0; --bit)
            {
                uint inBit = (v >> bit) & 1;
                uint top = (crc >> 3) & 1;
                crc = ((crc << 1) | inBit) & 0xF;
                if (top == 1) crc ^= polynomial;
            }
        }
        return (byte)(crc << 4); // put crc on the left and pad 0s
    }
}
