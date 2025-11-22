using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Collections.Unicode;
using Random = UnityEngine.Random;

public class RandomValue : IDMXGenerator
{
    public DMXChannel channelStart = 0;
    public DMXChannel channelEnd = 1;
    public EquationNumber minValue = 0;
    public EquationNumber maxValue = 255;
    public EquationNumber updateDelay = 500; // millisecond
    private float timer = 0;
    public bool enableRandom = true;

    public virtual void Construct() { }
    public virtual void Deconstruct() { }

    public virtual void GenerateDMX(ref List<byte> dmxData)
    {
        if (!enableRandom) return;

        timer += UnityEngine.Time.deltaTime;
        if (timer < updateDelay * 0.001f) return;
        timer = 0;
        
        dmxData.EnsureCapacity(channelStart + (channelEnd - channelStart) + 1);

        //we need to write to the dmx data list directly
        for (int i = channelStart; i < channelEnd + 1; i++)
        {
            dmxData[i] = (byte)Random.Range(minValue, maxValue);
        }
    }

    private protected TMP_InputField channelStartInputfield;
    private protected TMP_InputField channelEndInputfield;
    private protected TMP_InputField minValueInputfield;
    private protected TMP_InputField maxValueInputfield;
    private protected TMP_InputField updateDelayInputfield;
    private protected Toggle toggle;
    
    public virtual void ConstructUserInterface(RectTransform rect)
    {
        channelStartInputfield = Util.AddInputField(rect, "Channel Start")
            .WithText(channelStart)
            .WithCallback((value) => { channelStart = value; });

        channelEndInputfield = Util.AddInputField(rect, "Channel End")
            .WithText(channelEnd)
            .WithCallback((value) => { channelEnd = value; });

        minValueInputfield = Util.AddInputField(rect, "Min Value")
            .WithText(minValue)
            .WithCallback((value) => { minValue = value; });

        maxValueInputfield = Util.AddInputField(rect, "Max Value")
            .WithText(maxValue)
            .WithCallback((value) => { maxValue = value; });

        updateDelayInputfield = Util.AddInputField(rect, "Update delay")
            .WithText(updateDelay)
            .WithCallback((value) => { updateDelay = value; });

        toggle = Util.AddToggle(rect, "Enable")
            .WithValue(enableRandom)
            .WithCallback((value) => { enableRandom = value; });
    }

    public void DeconstructUserInterface() { }
    public void UpdateUserInterface() { }
}
