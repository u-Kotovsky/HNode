using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;

public class ChannelRemapper : MonoBehaviour
{
    public Loader loader;
    void Start()
    {
        //LoadPrefs();
        //debug remapping
        //mappings.Add(new ChannelMapping(0, 512*5, 13)); // Example mapping: Copy from channel 0 to channel 1 and 2
    }

    public void RemapChannels(ref List<byte> channels)
    {
        Profiler.BeginSample("Channel Remap");
        var mappings = loader.showconf.mappingsChannels;
        int maximumNewChannel = channels.Count;
        foreach (var mapping in mappings)
        {
            // Find the maximum target channel to ensure the list is large enough
            if (mapping.TargetChannel + mapping.SourceChannelLength > maximumNewChannel)
            {
                maximumNewChannel = mapping.TargetChannel + mapping.SourceChannelLength;
            }
        }

        // Create a temporary list to hold the remapped values
        List<byte> remappedChannels = new List<byte>(new byte[maximumNewChannel]);

        // Initialize the remapped channels with the original values
        for (int i = 0; i < channels.Count; i++)
        {
            remappedChannels[i] = channels[i];
        }

        // Apply the mappings
        foreach (var mapping in mappings)
        {
            // Copy the source channels to the target channel
            for (int i = 0; i < mapping.SourceChannelLength; i++)
            {
                int sourceIndex = mapping.SourceChannelStart + i;
                if (sourceIndex < channels.Count)
                {
                    remappedChannels[mapping.TargetChannel + i] = channels[sourceIndex];
                }
            }
        }

        // Replace the original channels with the remapped ones
        channels = remappedChannels;
        Profiler.EndSample();
    }

    public struct ChannelMapping
    {
        public int SourceChannelStart { get; set; }
        public int SourceChannelLength { get; set; }
        public int TargetChannel { get; set; }
        
        public ChannelMapping(int sourceChannelStart, int targetChannel, int sourceChannelLength = 1)
        {
            SourceChannelStart = sourceChannelStart;
            TargetChannel = targetChannel;
            SourceChannelLength = sourceChannelLength;
        }
    }
}
