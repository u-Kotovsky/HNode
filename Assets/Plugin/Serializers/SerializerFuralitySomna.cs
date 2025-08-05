using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using static TextureWriter;

public class FuralitySomna : IDMXSerializer
{
    const int blockSize = 16; // 10x10 pixels per channel block
    const int blocksPerCol = 13; // channels per column
    public Dictionary<string, ColorChannel> mergedChannels = new Dictionary<string, ColorChannel>();
    private Dictionary<int, ColorChannel> _mergedChannels = new Dictionary<int, ColorChannel>();
    
    int cumulativeOFfset = 0;

    public void Construct()
    {
        //convert the strings to integers by trying to parse them as a equation
        _mergedChannels.Clear();

        //TODO: Just make this whole thing a custom expression type that can be constructed from a string
        foreach (var channel in mergedChannels.Keys)
        {
            //this is dirty as fuck but whatever
            DataTable dt = new DataTable();
            var val = dt.Compute(channel, "");

            int valu = Int32.Parse(val.ToString());

            _mergedChannels.Add(valu, mergedChannels[channel]);
        }
    }
    public void Deconstruct() { }
    
    public void InitFrame()
    {
        cumulativeOFfset = 0;
    }
    public void CompleteFrame(ref Color32[] pixels, ref List<byte> channelValues, int textureWidth, int textureHeight) { }
    public void SerializeChannel(ref Color32[] pixels, byte channelValue, int channel, int textureWidth, int textureHeight)
    {
        int x = ((channel - cumulativeOFfset) / blocksPerCol) * blockSize;
        int y = ((channel - cumulativeOFfset) % blocksPerCol) * blockSize;

        if (_mergedChannels.ContainsKey(channel))
        {
            ColorChannel channelType = _mergedChannels[channel];
            TextureWriter.MixColorBlock(ref pixels, x, y, channelValue, channelType, blockSize);
        }
        else
        {
            var color = new Color32(
                channelValue,
                channelValue,
                channelValue,
                Util.GetBlockAlpha(channelValue)
            );
            TextureWriter.MakeColorBlock(ref pixels, x, y, color, blockSize);
        }

        if (_mergedChannels.ContainsKey(channel))
        {
            //if its blue, dont increment the offset
            if (_mergedChannels[channel] == ColorChannel.Blue)
            {
                return;
            }
            cumulativeOFfset++;
        }
    }

    public void DeserializeChannel(Texture2D tex, ref byte channelValue, int channel, int textureWidth, int textureHeight) => throw new NotImplementedException();
}
