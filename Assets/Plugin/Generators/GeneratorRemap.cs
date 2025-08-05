using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Remap : IDMXGenerator
{
    public DMXChannel SourceChannelStart = 0;
    public EquationNumber SourceChannelLength = 0;
    public DMXChannel TargetChannel = 0;
    public void Construct() { }
    public void Deconstruct() { }
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

    private protected TMP_InputField sourceChannelStartInputfield;
    private protected TMP_InputField sourceChannelLengthInputfield;
    private protected TMP_InputField targetChannelInputfield;
    public void ConstructUserInterface(RectTransform rect)
    {
        sourceChannelStartInputfield = Util.AddInputField(rect, "Source Channel Start");
        sourceChannelStartInputfield.text = SourceChannelStart.ToString();
        sourceChannelStartInputfield.onEndEdit.AddListener((value) =>
        {
            SourceChannelStart = value;
        });

        sourceChannelLengthInputfield = Util.AddInputField(rect, "Source Channel Length");
        sourceChannelLengthInputfield.text = SourceChannelLength.ToString();
        sourceChannelLengthInputfield.onEndEdit.AddListener((value) =>
        {
            SourceChannelLength = value;
        });

        targetChannelInputfield = Util.AddInputField(rect, "Target Channel");
        targetChannelInputfield.text = TargetChannel.ToString();
        targetChannelInputfield.onEndEdit.AddListener((value) =>
        {
            TargetChannel = value;
        });
    }

    public void DeconstructUserInterface() { }
    public void UpdateUserInterface() { }

}
