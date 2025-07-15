using System;
using System.Collections.Generic;
using UnityEngine;

public interface IDMXSerializer
{
    /// <summary>
    /// Serializes a channel from a raw byte representation, to a output video stream.
    /// </summary>
    /// <param name="pixels"></param>
    /// <param name="channelValue"></param>
    /// <param name="channel"></param>
    /// <param name="textureWidth"></param>
    /// <param name="textureHeight"></param>
    void SerializeChannel(ref Color32[] pixels, byte channelValue, int channel, int textureWidth, int textureHeight);

    /// <summary>
    /// Deserializes a channel from a input video stream, to a raw byte representation.
    /// </summary>
    /// <param name="tex"></param>
    /// <param name="channelValue"></param>
    /// <param name="channel"></param>
    /// <param name="textureWidth"></param>
    /// <param name="textureHeight"></param>
    void DeserializeChannel(Texture2D tex, ref byte channelValue, int channel, int textureWidth, int textureHeight);

    /// <summary>
    /// Called at the start of each frame to reset any state.
    /// </summary>
    void InitFrame();

    /// <summary>
    /// Called after all channels have been serialized for the current frame.
    /// Can be used to for example generate a CRC block area, or operate on multiple channels at once.
    /// </summary>
    /// <param name="pixels"></param>
    /// <param name="channelValues"></param>
    void CompleteFrame(ref Color32[] pixels, ref List<byte> channelValues);

    /// <summary>
    /// Called to initialize the serializer
    /// </summary>
    void Construct();
}
