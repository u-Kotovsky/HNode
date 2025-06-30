using System;
using UnityEngine;

public interface IDMXSerializer
{
    void SerializeChannel(ref Color32[] pixels, byte channelValue, int channel, int textureWidth, int textureHeight);
    void DeserializeChannel(Color[] pixels, ref byte channelValue, int channel, int textureWidth, int textureHeight);
    void InitFrame();
}
