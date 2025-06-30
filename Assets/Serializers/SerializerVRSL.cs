using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRSL : IDMXSerializer
{
    const int blockSize = 16; // 10x10 pixels per channel block
    const int blocksPerCol = 13; // channels per column

    public void InitFrame() { }

    public void SerializeChannel(ref Color32[] pixels, byte channelValue, int channel, int textureWidth, int textureHeight)
    {
        int universe = channel / 512; // Assuming 512 channels per universe
        int channelInUniverse = channel % 512; // Channel within the universe

        int x = (channelInUniverse / blocksPerCol) * blockSize;
        int y = (channelInUniverse % blocksPerCol) * blockSize;

        //stupid universe bullshit in VRSL
        int universeOffset = universe * (512 / blocksPerCol * blockSize) + (universe * blockSize);

        //convert the x y to pixel index
        //return 4x4 area
        var color = new Color32(
            channelValue,
            channelValue,
            channelValue,
            255
        );
        TextureWriter.MakeColorBlock(ref pixels, x + universeOffset, y, color, blockSize);
    }

    public void DeserializeChannel(Color[] pixels, ref byte channelValue, int channel, int textureWidth, int textureHeight)
    {
        int universe = channel / 512; // Assuming 512 channels per universe
        int channelInUniverse = channel % 512; // Channel within the universe

        int x = (channelInUniverse / blocksPerCol) * blockSize;
        int y = (channelInUniverse % blocksPerCol) * blockSize;

        //stupid universe bullshit in VRSL
        int universeOffset = universe * (512 / blocksPerCol * blockSize) + (universe * blockSize);

        //add a half offset to get the center
        x += blockSize / 2;
        y += blockSize / 2;

        // Get the color block from the texture
        Color32 color = TextureReader.GetColor(pixels, x + universeOffset, y);

        // Convert the color block to a channel value
        channelValue = color.g;
    }
}
