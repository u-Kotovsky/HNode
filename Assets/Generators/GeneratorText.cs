using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.Unicode;

public class Text : IDMXGenerator
{
    public string text = "";
    /// <summary>
    /// The channel to start writing the text to. The text will be written starting at this channel and continuing until the text is fully written or <see cref="maxCharacters"/> is reached if <see cref="limitLength"/> is true.
    /// </summary>
    public int channelStart = 0;
    /// <summary>
    /// If true, the text will be encoded as UTF-16 (Unicode). If false, the text will be encoded as UTF-8.
    /// </summary>
    public bool unicode = false;
    /// <summary>
    /// If true, the text will be limited to <see cref="maxCharacters"/> channels. If false, the text will be written until it is fully written.
    /// </summary>
    public bool limitLength = false;
    /// <summary>
    /// The maximum length of the text. Any additional channels that would be written will be ignored if <see cref="limitLength"/> is true.
    /// </summary>
    public int maxCharacters = 32;

    public virtual void Construct() { }

    //I swear to god to anyone reading this code. This is like, the 5TH FUCKING TIME ive had to write a system like this
    //its unbelievable that there isnt a standard for fallback character conversion considering the ammount of times
    //its been a issue in my codebases. Seriously WTF
    //This one is based from scratcher bot
    public static readonly Dictionary<char, string> CleanupTable = new() //WARNING! Make sure there is no duplicate keys or it will cause a runtime error
    {
        { '‘', "\'" },
        { '’', "\'" },
        { '“', "\"" },
        { '”', "''" },
        { '„', ",," },
        { '‟', "\"" },
        { '‹', "<" },
        { '›', ">" },
        { '‚', "," },
        { '‛', "'" },
        { '¡', "!" },
        { '¿', "?" },
        { '‼', "!!" },
        { '⁇', "??" },
        { '⁈', "?!" },
        { '⁉', "!?" },
        { '‽', "?!" },
        { '†', "+" },
        { '‡', "++" },
        { '․', "." },
        { '‥', ".." },
        { '…', "..." },
        { '‰', "%" },
        { '‱', "%%" },
        { '′', "'" },
        { '″', "\"" },
        { '‴', "\"" },
        { '‵', "`" },
        { '‶', "\"" },
        { '‷', "\"" },
        { '‸', "^" },
        { '※', "*" },
        { '⁂', "***" },
        { '⁄', "/" },
        { '⁎', "*" },
        { '⁏', ";" },
        { '⁒', "%" },
        { '⁓', "~" },
        { '⁕', "*" },
        { '–', "-" },
        { '—', "-" },
        { '​', " " },
        //{ new Rune((char)55357, (char)56898), ":)" }, // :)
        //{ new Rune((char)55357, (char)56870), ":(" }, // :)
    };

    public virtual void GenerateDMX(ref List<byte> dmxData)
    {
        if (!unicode)
        {
            //try to downconvert first for common characters
            string cleaned = "";
            foreach (char c in text)
            {
                if (CleanupTable.TryGetValue(c, out string charValue))
                {
                    cleaned += charValue;
                }
                else
                {
                    cleaned += c;
                }
            }
            text = cleaned;
        }
        
        //trim to max characters if the control is present
        if (limitLength && text.Length > maxCharacters)
        {
            text = text.Substring(0, maxCharacters);
        }

        //convert text to DMX data
        byte[] textBytes = new byte[0];
        if (unicode)
        {
            //unicode implementation
            textBytes = System.Text.Encoding.Unicode.GetBytes(text);
        }
        else
        {
            //utf8 implementation
            textBytes = System.Text.Encoding.UTF8.GetBytes(text);
        }

        dmxData.EnsureCapacity(channelStart + textBytes.Length);

        //we need to write to the dmx data list directly
        for (int i = channelStart; i < channelStart + textBytes.Length; i++)
        {
            dmxData[i] = textBytes[i - channelStart];
        }
    }
}
