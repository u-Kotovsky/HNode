using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Collections.Unicode;

public class StaticValue : IDMXGenerator
{
    public DMXChannel channelStart = 0;
    public DMXChannel channelEnd = 1;
    public EquationNumber value = 0;

    public virtual void Construct() { }
    public virtual void Deconstruct() { }

    public virtual void GenerateDMX(ref List<byte> dmxData)
    {
        dmxData.EnsureCapacity(channelStart + (channelEnd - channelStart));

        //we need to write to the dmx data list directly
        for (int i = channelStart; i < channelEnd + 1; i++)
        {
            dmxData[i] = (byte)value;
        }
    }

    private protected TMP_InputField channelStartInputfield;
    private protected TMP_InputField channelEndInputfield;
    private protected TMP_InputField valueInputfield;
    public virtual void ConstructUserInterface(RectTransform rect)
    {
        channelStartInputfield = Util.AddInputField(rect, "Channel Start")
            .WithText(channelStart)
            .WithCallback((value) => { channelStart = value; });

        channelEndInputfield = Util.AddInputField(rect, "Channel End")
            .WithText(channelEnd)
            .WithCallback((value) => { channelEnd = value; });

        valueInputfield = Util.AddInputField(rect, "Value")
            .WithText(value)
            .WithCallback((value) => { this.value = value; });
    }

    public void DeconstructUserInterface()
    {
        //throw new NotImplementedException();
    }

    public void UpdateUserInterface()
    {

    }
}
