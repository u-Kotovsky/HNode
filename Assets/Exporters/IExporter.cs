using System;
using System.Collections.Generic;
using UnityEngine;

public interface IExporter
{
    /// <summary>
    /// Serializes a channel from a raw byte representation
    /// </summary>
    /// <param name="channelValue"></param>
    /// <param name="channel"></param>
    void SerializeChannel(byte channelValue, int channel);

    //todo, implement this later as something they can allow
    /*     /// <summary>
        /// Deserializes a channel from a input
        /// </summary>
        /// <param name="channelValue"></param>
        /// <param name="channel"></param>
        void DeserializeChannel(ref byte channelValue, int channel); */

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
    void CompleteFrame(ref List<byte> channelValues);

    /// <summary>
    /// Called to initialize the exporter
    /// </summary>
    void Construct();
    
    void Deconstruct();
}
