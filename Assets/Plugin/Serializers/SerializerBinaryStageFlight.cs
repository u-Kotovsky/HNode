using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;

public class BinaryStageFlight : IDMXSerializer
{
    private int blockSize = 4; // pixels per channel block

    private int channelsPerCol = 6;
    private int ChannelsPerCol
    {
        get => channelsPerCol;
        set
        {
            channelsPerCol = value;
            blocksPerCol = value * 8;
        }
    }

    private int blocksPerCol = 6 * 8; // channels per column
    private int BlocksPerCol
    {
        get => channelsPerCol;
        set
        {
            blocksPerCol = value;
            channelsPerCol = value / 8;
        }
    }
    private int CRCBits = 4;

    public void Construct() { }
    public void Deconstruct() { }
    public void InitFrame(ref List<byte> channelValues) { }
    public void CompleteFrame(ref Color32[] pixels, ref List<byte> channelValues, int textureWidth, int textureHeight)
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
            for (int j = 0; j < /* bits.Length */ CRCBits; j++)
            {
                int y = startY + j * blockSize;
                CalculateWrapping(x, y, out int xd, out int yd, textureWidth);
                //convert the x y to pixel index
                //return 4x4 area
                var color = new Color32(
                    (byte)(bits[7 - j] ? 255 : 0),
                    (byte)(bits[7 - j] ? 255 : 0),
                    (byte)(bits[7 - j] ? 255 : 0),
                    /* (byte)(bits[j] ? 255 : 0),
                    (byte)(bits[j] ? 255 : 0),
                    (byte)(bits[j] ? 255 : 0), */
                    Util.GetBlockAlpha(255) // Alpha should be forced on always
                );
                TextureWriter.MakeColorBlock(ref pixels, xd, yd, color, blockSize);
            }
        }
    }

    public void SerializeChannel(ref Color32[] pixels, byte channelValue, int channel, int textureWidth, int textureHeight)
    {
        //split the value into 8 bits
        //sane endianness
        for (int i = 0; i < 8; i++)
        {
            GetPositionData(channel, i, textureWidth, out int x, out int y);
            /* if (x >= textureWidth || y >= textureHeight)
            {
                continue; // Skip if the calculated pixel is out of bounds
            } */
            //convert the x y to pixel index
            //return 4x4 area
            byte val = (byte)(GetBitFromByte(channelValue, i) ? 255 : 0);
            var color = new Color32(
                val,
                val,
                val,
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
            GetPositionData(channel, i, textureWidth, out int x, out int y);
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

    #region Helpers
    private bool GetBitFromByte(byte value, int bitIndex)
    {
        return (value & (1 << bitIndex)) != 0;
    }
    
    private void GetPositionData(int channel, int i, int textureWidth, out int x, out int y)
    {
        //int newChannel = (channel * 8) + i;
        //encode backwards, endiannes flip
        int newChannel = (channel * 8) + (7 - i);
        x = (newChannel / blocksPerCol) * blockSize;
        y = (newChannel % blocksPerCol) * blockSize;
        CalculateWrapping(x, y, out x, out y, textureWidth);
    }

    private void CalculateWrapping(int x, int y, out int adjx, out int adjy, int textureWidth)
    {
        int wrap = x / textureWidth;
        adjx = x % textureWidth;
        adjy = y + (wrap * (blocksPerCol + CRCBits) * blockSize); // +4 is for the CRC bits
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

    public byte Crc4(params byte[] data)
    {
        uint crc = 0u;
        uint polynomial = 0x03;

        foreach (uint v in data)
        {
            for (int bit = 7; bit >= 0; --bit)
            {
                uint inBit = (v >> bit) & 1u;
                bool top = (crc & 0x8u) != 0u;
                crc = ((crc << 1) | inBit) & 0xFu;
                if (top) crc ^= polynomial;
            }
        }
        return (byte)(crc << CRCBits); // put crc on the left and pad 0s
    }
    #endregion

    #region UserInterface
    private protected TMP_InputField blockSizeInputfield;
    private protected TMP_InputField channelPerColInputfield;
    private protected TMP_InputField blocksPerColInputfield;
    private protected TMP_InputField CRCBitsInputfield;

    public void ConstructUserInterface(RectTransform rect)
    {
        blockSizeInputfield = Util.AddInputField(rect, "Block size")
            .WithText(blockSize.ToString())
            .WithCallback(value => { int.TryParse(value, out blockSize); });
        
        channelPerColInputfield = Util.AddInputField(rect, "Channels per column")
            .WithText(channelsPerCol.ToString())
            .WithCallback(value => 
            {
                int.TryParse(value, out var result);
                ChannelsPerCol = result;
            });
        
        blocksPerColInputfield = Util.AddInputField(rect, "Blocks per column")
            .WithText(blocksPerCol.ToString())
            .WithCallback(value =>
            {
                int.TryParse(value, out var result);
                ChannelsPerCol = result;
            });
        
        CRCBitsInputfield = Util.AddInputField(rect, "CRC bits")
            .WithText(CRCBits.ToString())
            .WithCallback(value =>
            {
                int.TryParse(value, out CRCBits);
            });
    }

    public void DeconstructUserInterface() { }

    public void UpdateUserInterface() { }

    #endregion
}