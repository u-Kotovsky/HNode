using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRSL : IDMXSerializer
{
    const int blockSize = 16; // 10x10 pixels per channel block
    const int blocksPerCol = 13; // channels per column
    
    /// <summary>
    /// Automatically mask channels that are set to zero.
    /// </summary>
    public bool autoMaskOnZero { get; set; }

    public void Construct() { }
    public void InitFrame() { }

    public void SerializeChannel(ref Color32[] pixels, byte channelValue, int channel, int textureWidth, int textureHeight)
    {
        GetPositionData(channel, out int x, out int y, out int universeOffset);

        //convert the x y to pixel index
        //return 4x4 area
        var color = new Color32(
            channelValue,
            channelValue,
            channelValue,
            autoMaskOnZero && channelValue == 0 ? (byte)0 : (byte)255 // if autoMaskOnZero is true and channelValue is 0, set alpha to 0
        );
        TextureWriter.MakeColorBlock(ref pixels, x + universeOffset, y, color, blockSize);
    }

    public void DeserializeChannel(Texture2D tex, ref byte channelValue, int channel, int textureWidth, int textureHeight)
    {
        GetPositionData(channel, out int x, out int y, out int universeOffset);

        //add a half offset to get the center
        x += blockSize / 2;
        y += blockSize / 2;

        // Get the color block from the texture
        Color32 color = TextureReader.GetColor(tex, x + universeOffset, y);

        // Convert the color block to a channel value
        channelValue = color.g;
    }

    private static void GetPositionData(int channel, out int x, out int y, out int universeOffset)
    {
        int universe = channel / 512; // Assuming 512 channels per universe
        int channelInUniverse = channel % 512; // Channel within the universe

        x = (channelInUniverse / blocksPerCol) * blockSize;
        y = (channelInUniverse % blocksPerCol) * blockSize;

        //stupid universe bullshit in VRSL
        universeOffset = universe * (512 / blocksPerCol * blockSize) + (universe * blockSize);
    }
}
