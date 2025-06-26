using System;
using UnityEngine;

interface IDMXSerializer
{
    void MapChannel(ref Color32[] pixels, byte channelValue, int channel, int textureWidth, int textureHeight);

    void InitFrame();
}
