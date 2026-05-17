using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GeneratorSin : IDMXGenerator
{
    public DMXChannel channelStart = 0;
    public DMXChannel channelEnd = 1;
    public float scale = 32;
    public float speed = 1;
    public float lower = 1;
    public float upper = 2;
    
    private protected TMP_InputField channelStartInputfield;
    private protected TMP_InputField channelEndInputfield;
    private protected TMP_InputField scaleInputfield;
    private protected TMP_InputField speedInputfield;
    
    public void GenerateDMX(ref List<byte> dmxData)
    {
        dmxData.EnsureCapacity(channelStart + (channelEnd - channelStart) + 1);

        float time = UnityEngine.Time.time * speed;
        float len = channelEnd - channelStart;

        //we need to write to the dmx data list directly
        for (int i = channelStart; i < channelEnd + 1; i++)
        {
            dmxData[i] = (byte)((lower + Mathf.Sin(time + ((float)i / len) * scale))/upper*255);
        }
    }
    
    public void ConstructUserInterface(RectTransform rect)
    {
        channelStartInputfield = Util.AddInputField(rect, "Channel Start")
            .WithText(channelStart)
            .WithCallback((value) => { channelStart = value; });

        channelEndInputfield = Util.AddInputField(rect, "Channel End")
            .WithText(channelEnd)
            .WithCallback((value) => { channelEnd = value; });

        channelEndInputfield = Util.AddInputField(rect, "Scale")
            .WithText(scale.ToString())
            .WithCallback((value) => { float.TryParse(value, out scale); });

        channelEndInputfield = Util.AddInputField(rect, "Speed")
            .WithText(speed.ToString())
            .WithCallback((value) => { float.TryParse(value, out speed); });

        channelEndInputfield = Util.AddInputField(rect, "Lower")
            .WithText(lower.ToString())
            .WithCallback((value) => { float.TryParse(value, out lower); });

        channelEndInputfield = Util.AddInputField(rect, "Upper")
            .WithText(upper.ToString())
            .WithCallback((value) => { float.TryParse(value, out upper); });
    }

    public void DeconstructUserInterface() { }
    public void UpdateUserInterface() { }
    public void Construct() { }
    public void Deconstruct() { }

}
