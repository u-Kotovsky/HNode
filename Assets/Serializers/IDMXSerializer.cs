using System;
using UnityEngine;

interface IDMXSerializer
{
    static void MapChannel(ref Color32[] pixels, byte channelValue, int channel, int textureWidth, int textureHeight) => throw new NotImplementedException();
}
