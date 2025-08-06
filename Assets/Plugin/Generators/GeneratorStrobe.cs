using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Strobe : IDMXGenerator
{
    public DMXChannel channel = 0;
    public byte valueOn = 255;
    public byte valueOff = 0;
    public float frequency = 1.0f; //in Hz

    public void Construct() { }
    public void Deconstruct() { }
    public void GenerateDMX(ref List<byte> dmxData)
    {
        dmxData.EnsureCapacity(channel + 1);

        //use system time to determine if we are on or off
        var time = DateTime.Now.Millisecond;

        //now determine if we are on or off
        float period = 1000.0f / frequency; //in ms

        if (time % period < period / 2)
        {
            dmxData[channel] = valueOn;
        }
        else
        {
            dmxData[channel] = valueOff;
        }
    }

    private protected TMP_InputField channelInputfield;
    private protected TMP_InputField valueOnInputfield;
    private protected TMP_InputField valueOffInputfield;
    private protected TMP_InputField frequencyInputfield;
    public void ConstructUserInterface(RectTransform rect)
    {
        channelInputfield = Util.AddInputField(rect, "Channel")
            .WithText(channel)
            .WithCallback((value) => { channel = value; });

        valueOnInputfield = Util.AddInputField(rect, "Value On")
            .WithText(valueOn.ToString())
            .WithCallback((value) =>
                {
                    if (byte.TryParse(value, out byte newValue)) { valueOn = newValue; }
                    else { Debug.LogWarning("Invalid value for 'On', must be a byte."); }
                })
            .WithContentType(TMP_InputField.ContentType.IntegerNumber);

        valueOffInputfield = Util.AddInputField(rect, "Value Off")
            .WithText(valueOff.ToString())
            .WithCallback((value) =>
                {
                    if (byte.TryParse(value, out byte newValue)) { valueOff = newValue; }
                    else { Debug.LogWarning("Invalid value for 'Off', must be a byte."); }
                })
            .WithContentType(TMP_InputField.ContentType.IntegerNumber);

        frequencyInputfield = Util.AddInputField(rect, "Frequency (Hz)")
            .WithText(frequency.ToString())
            .WithCallback((value) =>
                {
                    if (float.TryParse(value, out float newFrequency) && newFrequency > 0)
                    {
                        frequency = newFrequency;
                    }
                    else
                    {
                        Debug.LogWarning("Invalid frequency input, must be a positive number.");
                    }
                })
            .WithContentType(TMP_InputField.ContentType.DecimalNumber);
    }

    public void DeconstructUserInterface()
    {
        //throw new NotImplementedException();
    }

    public void UpdateUserInterface()
    {

    }
}
