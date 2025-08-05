using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Remap : IDMXGenerator
{
    public DMXChannel SourceChannelStart { get; set; }
    public EquationNumber SourceChannelLength { get; set; }
    public DMXChannel TargetChannel { get; set; }
    public void GenerateDMX(ref List<byte> dmxData)
    {
        //ensure capacity
        dmxData.EnsureCapacity(TargetChannel + (int)SourceChannelLength);

        //copy the source channels to the target channel
        for (int i = 0; i < SourceChannelLength; i++)
        {
            dmxData[TargetChannel + i] = dmxData[SourceChannelStart + i];
        }
    }

    public void ConstructUserInterface(RectTransform rect) { }
    public void DeconstructUserInterface() { }

    public void UpdateUserInterface() { }

    public void Construct() { }

    public void Deconstruct() { }
}
