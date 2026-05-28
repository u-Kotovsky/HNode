using System.Collections.Generic;
using AudioLink;
using TMPro;
using UnityEngine;

public class FixtureAudioLink : IDMXGenerator
{
    private GeneratorAudioLink _generatorAudioLink;

    private DMXChannel _startChannel = 0;
    
    private TMP_InputField _startChannelInputField;
    
    [Header("AudioLink Settings")]
    public AudioLinkBand band;
    public AudioReactiveLightColorMode colorMode = AudioReactiveLightColorMode.STATIC;
    public bool smooth;
    [Range(0, 127)]
    public int delay;

    [Header("Reactivity Settings")]
    public bool affectIntensity = true;
    public float intensityMultiplier = 1f;
    public float hueShift;
    
    // TODO: optional controls for each dmx channel (for ex. pan/tilt sin/cos)
    
    public void ConstructUserInterface(RectTransform rect)
    {
        _startChannelInputField = Util.AddInputField(rect, "Start Channel")
            .WithText(_startChannel)
            .WithCallback(value => { if (int.TryParse(value, out var index)) _startChannel = index; });
    }

    public void DeconstructUserInterface()
    {
        
    }

    public void UpdateUserInterface()
    {
        
    }

    public void Construct()
    {
        // todo: load config from file

        _generatorAudioLink = GeneratorAudioLink.GetInstance();
    }

    public void Deconstruct()
    {
        
    }
    
    public void GenerateDMX(ref List<byte> dmxData)
    {
        dmxData.EnsureCapacity(_startChannel + 128);

        if (_generatorAudioLink == null)
        {
            return;
        }

        if (!_generatorAudioLink.AudioLink.AudioDataIsAvailable())
        {
            return;
        }

        for (int i = 0; i < 127; i++)
        {
            float amplitude = AudioLink.AudioLink.ToGrayscale(_generatorAudioLink.AudioLink.GetBandAsSmooth(band, i, smooth));
            dmxData[_startChannel + i] = (byte)(amplitude * 255);
        }
    }
    
    private Color HueShift(Color color, float hueShiftAmount)
    {
        float h, s, v;
        Color.RGBToHSV(color, out h, out s, out v);
        h += hueShiftAmount;
        return Color.HSVToRGB(h, s, v);
    }
}
