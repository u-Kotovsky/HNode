using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Time : Text
{
    public string format = "HH:mm:ss";
    public override void GenerateDMX(ref List<byte> dmxData)
    {
        //construct a text
        text = DateTime.Now.ToString(format);

        //call base now that we have filled the text
        base.GenerateDMX(ref dmxData);
    }

    private protected TMP_InputField formatInputfield;

    public override void ConstructUserInterface(RectTransform rect)
    {
        base.ConstructUserInterface(rect);

        //disable interaction
        if (textInputfield != null)
        {
            textInputfield.interactable = false;
        }

        formatInputfield = Util.AddInputField(rect, "Format")
            .WithText(format)
            .WithCallback((value) => { format = value; });
    }
}
