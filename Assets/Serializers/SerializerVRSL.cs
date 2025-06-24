using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRSL : IDMXSerializer
{
    public void MapChannel(ref Color32[] pixels, byte channelValue, int channel, int textureWidth, int textureHeight)
    {
        //convert the channel to x y
        const int blockSize = 16; // 10x10 pixels per channel block
        const int blocksPerCol = 13; // channels per column


        int universe = channel / 512; // Assuming 512 channels per universe
        int channelInUniverse = channel % 512; // Channel within the universe

        int x = (channelInUniverse / blocksPerCol) * blockSize;
        int y = (channelInUniverse % blocksPerCol) * blockSize;

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
}
