using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Strobe : IDMXGenerator
{
    public int channel = 0;
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
        channelInputfield = Util.AddInputField(rect, "Channel");
        channelInputfield.text = channel.ToString();
        channelInputfield.contentType = TMP_InputField.ContentType.IntegerNumber;
        channelInputfield.onEndEdit.AddListener((value) =>
        {
            if (int.TryParse(value, out int newChannel))
            {
                channel = newChannel;
            }
            else
            {
                Debug.LogWarning("Invalid channel input, must be an integer.");
            }
        });

        valueOnInputfield = Util.AddInputField(rect, "Value On");
        valueOnInputfield.text = valueOn.ToString();
        valueOnInputfield.contentType = TMP_InputField.ContentType.IntegerNumber;
        valueOnInputfield.onEndEdit.AddListener((value) =>
        {
            if (byte.TryParse(value, out byte newValue))
            {
                valueOn = newValue;
            }
            else
            {
                Debug.LogWarning("Invalid value for 'On', must be a byte.");
            }
        });

        valueOffInputfield = Util.AddInputField(rect, "Value Off");
        valueOffInputfield.text = valueOff.ToString();
        valueOffInputfield.contentType = TMP_InputField.ContentType.IntegerNumber;
        valueOffInputfield.onEndEdit.AddListener((value) =>
        {
            if (byte.TryParse(value, out byte newValue))
            {
                valueOff = newValue;
            }
            else
            {
                Debug.LogWarning("Invalid value for 'Off', must be a byte.");
            }
        });

        frequencyInputfield = Util.AddInputField(rect, "Frequency (Hz)");
        frequencyInputfield.text = frequency.ToString();
        frequencyInputfield.contentType = TMP_InputField.ContentType.DecimalNumber;
        frequencyInputfield.onEndEdit.AddListener((value) =>
        {
            if (float.TryParse(value, out float newFrequency) && newFrequency > 0)
            {
                frequency = newFrequency;
            }
            else
            {
                Debug.LogWarning("Invalid frequency input, must be a positive number.");
            }
        });
    }

    public void DeconstructUserInterface()
    {
        //throw new NotImplementedException();
    }

    public void UpdateUserInterface()
    {
        
    }
}
