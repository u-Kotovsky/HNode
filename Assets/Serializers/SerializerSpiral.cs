using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spiral : IDMXSerializer
{
    const int blockSize = 8; // 10x10 pixels per channel block
    int x = 0;
    int y = 0;
    int state = 0;
    List<Vector2Int> visited = new List<Vector2Int>();
    public void InitFrame()
    {
        x = 0;
        y = 0;
        visited.Clear();
        state = 0;
    }

    public void SerializeChannel(ref Color32[] pixels, byte channelValue, int channel, int textureWidth, int textureHeight)
    {
        int scaledWidth = textureWidth / blockSize;
        int scaledHeight = textureHeight / blockSize;


        //multiply up by block size
        int xfinal = x * blockSize;
        int yfinal = y * blockSize;

        //convert the x y to pixel index
        //return 4x4 area
        var color = new Color32(
            channelValue,
            channelValue,
            channelValue,
            255
        );
        TextureWriter.MakeColorBlock(ref pixels, xfinal, yfinal, color, blockSize);

        int nextX = x;
        int nextY = y;
        CalculateNextMove(ref nextX, ref nextY);

        //self collision check
        if (visited.Contains(new Vector2Int(nextX, nextY)) ||
            nextX < 0 || nextY < 0 ||
            nextX >= scaledWidth || nextY >= scaledHeight)
        {
            //we've hit a wall, change direction
            state++;
            if (state > 3) state = 0;
            CalculateNextMove(ref nextX, ref nextY);
        }

        visited.Add(new Vector2Int(x, y));

        x = nextX;
        y = nextY;
    }

    private void CalculateNextMove(ref int nextX, ref int nextY)
    {
        nextX = x;
        nextY = y;
        //do thing based on state
        switch (state)
        {
            case 0:
                nextX = x + 1;
                break;
            case 1:
                nextY = y + 1;
                break;
            case 2:
                nextX = x - 1;
                break;
            case 3:
                nextY = y - 1;
                break;
        }
    }
    
    public void DeserializeChannel(Color[] pixels, ref byte channelValue, int channel, int textureWidth, int textureHeight) => throw new NotImplementedException();
}
