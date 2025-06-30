using System;
using UnityEngine;

public interface IDMXSerializer
{
    void SerializeChannel(ref Color32[] pixels, byte channelValue, int channel, int textureWidth, int textureHeight);
    void DeserializeChannel(Texture2D tex, ref byte channelValue, int channel, int textureWidth, int textureHeight);
    void InitFrame();
}
