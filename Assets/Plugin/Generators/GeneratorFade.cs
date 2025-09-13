using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Collections.Unicode;

//quickly hacked together for a event but this should also generally be useful
// we need util functions for handling a list of a type though so some of its not UI exposed
public class Fade : IDMXGenerator
{
    public List<DMXChannel> channels = new List<DMXChannel>();
    public EquationNumber valueToFadeTo = 0;
    private TimeSpan fadeStart;
    private TimeSpan fadeEnd;
    public TimeSpan fadeDuration = TimeSpan.FromSeconds(5);

    public virtual void Construct() { }
    public virtual void Deconstruct() { }

    public virtual void GenerateDMX(ref List<byte> dmxData)
    {
        //get a 0 to 1 value based on the current time and the fade start and end times
        var now = DateTime.Now.TimeOfDay;
        float t = Mathf.InverseLerp((float)fadeStart.TotalMilliseconds, (float)fadeEnd.TotalMilliseconds, (float)now.TotalMilliseconds);
        //figure out the divisor
        //lerp between the current value
        //Debug.Log(t);
        //multiply each channel by t
        foreach (var channel in channels)
        {
            dmxData[channel] = (byte)Mathf.Lerp(dmxData[channel], valueToFadeTo, t);
        }
    }

    public virtual void ConstructUserInterface(RectTransform rect)
    {
        //make a button to trigger a fade in
        //fade in is where fadeEnd is before fadeStart
        //fade out is where fadeStart is before fadeEnd

        var fadeIn = Util.AddButton(rect, "Fade In")
            .WithCallback(() =>
            {
                fadeStart = DateTime.Now.TimeOfDay + fadeDuration;
                fadeEnd = DateTime.Now.TimeOfDay;
            });

        var fadeOut = Util.AddButton(rect, "Fade Out")
            .WithCallback(() =>
            {
                fadeStart = DateTime.Now.TimeOfDay;
                fadeEnd = DateTime.Now.TimeOfDay + fadeDuration;
            });

        //add a field to set the fade duration
        Util.AddInputField(rect, "Fade Duration (s)")
            .WithText(fadeDuration.TotalSeconds.ToString())
            .WithCallback((value) =>
            {
                if (double.TryParse(value, out double result))
                {
                    fadeDuration = TimeSpan.FromSeconds(result);
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
