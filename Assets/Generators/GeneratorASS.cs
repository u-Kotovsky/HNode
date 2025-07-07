using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ASS : Text
{
    //file path to ASS file
    public string filePath = "";

    public Mode mode = Mode.OnConfigLoad;
    private TimeSpan timeAtLoad = TimeSpan.Zero;

    private ASSFile file;

    public override void GenerateDMX(ref List<byte> dmxData)
    {
        //clear text
        text = "";

        //figure out the actual time using DateTime.Now.TimeOfDay
        TimeSpan timeIn = DateTime.Now.TimeOfDay - timeAtLoad;

        if (file.ActiveEvents(timeIn, out List<ASSEvent> activeEvs))
        {
            //get the active styles on this frame
            List<Style> styleList = file.GetActiveStyles(timeIn);

            //serialize to string deliminated by ^
            foreach (Style style in styleList)
            {
                text += style.ToEncodedString() + "^";
            }

            //divider for style vs text
            text += "@";
            int textStart = text.Length;

            foreach (ASSEvent e in activeEvs)
            {
                text += e.ToEncodedString() + "^";
            }

            //print, stripping out anything between {}
            //Debug.Log(text.Substring(textStart));
            //Debug.Log(text[textStart + 1]);
            string strippedText = text.Substring(textStart + 1);
            strippedText = "?" + strippedText;
            strippedText = System.Text.RegularExpressions.Regex.Replace(strippedText, @"{[\s\S]*?}", "(???)");
            //strippedText = System.Text.RegularExpressions.Regex.Replace(strippedText, @"^[\s\S]|", "?|");
            Debug.Log($"{text.Length} Bytes, Text: {strippedText}");
        }

        //call base now that we have filled the text
        base.GenerateDMX(ref dmxData);
    }

    //constructor, unusual but used to setup the initial file load
    public override void Construct()
    {
        Debug.Log("ASS Construct called, loading file: " + filePath);
        string lyricsraw = "";
        //load the file if it exists
        if (System.IO.File.Exists(filePath))
        {
            lyricsraw = System.IO.File.ReadAllText(filePath);
        }
        else
        {
            Debug.LogWarning("ASS file not found: " + filePath);
            lyricsraw = "";
        }

        file = new ASSFile(lyricsraw);

        Debug.Log($"File: {filePath} loaded\n{file.events.Count} Events, {file.styles.Count} Styles");

        //print the file
        Debug.Log(file);

        timeAtLoad = DateTime.Now.TimeOfDay;
    }

    public enum Mode
    {
        OnConfigLoad
    }

    private struct ASSFile
    {
        public string Title;
        public string ScriptType;
        public string Subtitler;
        public List<Style> styles;
        public List<ASSEvent> events;

        public override string ToString()
        {
            return $@"{Title}
{ScriptType}
{Subtitler}

[Styles]
{styles.ToNewlineString()}

[Events]
{events.ToNewlineString()}";
        }

        public ASSFile(string rawfile)
        {
            //first find the metadata
            string[] lines = rawfile.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            Title = "";
            ScriptType = "";
            Subtitler = "";

            //metadata should be at the top, so search for lines that have specific things as the start
            foreach (string line in lines)
            {
                if (line.StartsWith("Title: "))
                {
                    Title = line.Substring(7).Trim();
                }
                else if (line.StartsWith("ScriptType: "))
                {
                    ScriptType = line.Substring(12).Trim();
                }
                else if (line.StartsWith("Original Translation: "))
                {
                    Subtitler = line.Substring(22).Trim();
                }
            }

            //next we need to find the blocks that contain styles and events
            //styles will start with [V4+ Styles]
            styles = new List<Style>();

            int styleStartIndex = Array.FindIndex(lines, line => line.Trim() == "[V4+ Styles]") + 2; // +2 to skip the header and the next line that contains the format text
            int styleEndIndex = Array.FindIndex(lines, styleStartIndex, line => line.Trim().StartsWith("[Events]"));

            for (int i = styleStartIndex; i < styleEndIndex; i++)
            {
                if (lines[i].Trim().StartsWith("Style: "))
                {
                    //create a new style from the line
                    Style style = new Style(lines[i].Substring(7).Trim(), styles.Count);
                    styles.Add(style);
                }
            }

            events = new List<ASSEvent>();
            int eventStartIndex = Array.FindIndex(lines, line => line.Trim() == "[Events]") + 2; // +2 to skip the header and the next line that contains the format text
            int eventEndIndex = lines.Length; //assume the end is the end of the file

            for (int i = eventStartIndex; i < eventEndIndex; i++)
            {
                if (lines[i].Trim().StartsWith("Dialogue: "))
                {
                    //create a new event from the line
                    ASSEvent asevent = new ASSEvent(lines[i].Substring(10).Trim(), ref styles);
                    events.Add(asevent);
                }
            }

            //theoretically, we should have all the styles and events now
        }

        public bool ActiveEvents(TimeSpan currentTime, out List<ASSEvent> eventsOut)
        {
            //Debug.Log("Checking for active events at " + currentTime);
            eventsOut = new List<ASSEvent>();

            if (events == null)
            {
                return false;
            }

            //find the first event that is active
                for (int i = 0; i < events.Count; i++)
                {
                    if (events[i].IsActive(currentTime))
                    {
                        eventsOut.Add(events[i]);
                    }
                }

            return eventsOut.Count > 0;
        }

        public List<Style> GetActiveStyles(TimeSpan currentTime)
        {
            List<Style> activeStyles = new List<Style>();
            //find the styles of the active events
            foreach (ASSEvent e in events)
            {
                if (e.IsActive(currentTime) && !activeStyles.Contains(e.style))
                {
                    activeStyles.Add(e.style);
                }
            }
            return activeStyles;
        }
    }

    private struct ASSEvent
    {
        public int layer;
        public TimeSpan start;
        public TimeSpan end;
        public Style style;
        public int styleIndex; //index of the style in the styles list
        public string name;
        public float MarginL;
        public float MarginR;
        public float MarginV;
        public string Effect; //probably a enum
        public string text;

        public string ToEncodedString()
        {
            //return only the neccesary info, deliminated by |
            //we dont need time as we are handling that ourselves
            //margin is excluded because its not commonly used
            return $"{(char)styleIndex}|{text}";
        }

        public override string ToString()
        {
            return $@"{start} --> {end}: {text}
{style.name} (Layer: {layer}, MarginL: {MarginL}, MarginR: {MarginR}, MarginV: {MarginV}, Effect: {Effect})";
        }

        public bool IsActive(TimeSpan currentTime)
        {
            return currentTime >= start && currentTime <= end;
        }

        public ASSEvent(string rawLine, ref List<Style> styles)
        {
            //its assumed the raw line passed in here is with the "Dialogue: " removed
            string[] parts = rawLine.Split(',');

            //now we need to parse everything out
            layer = int.Parse(parts[0].Trim());
            //parse as h:mm:ss:xx with xx being hundredth of a second
            start = TimeSpan.ParseExact(parts[1].Trim(), @"h\:mm\:ss\.ff", null);
            end = TimeSpan.ParseExact(parts[2].Trim(), @"h\:mm\:ss\.ff", null);

            //style is a string reference in the line, so we need to find it in the styles list
            styleIndex = styles.FindIndex(s => s.name == parts[3].Trim());
            style = styles[styleIndex];
            name = parts[4].Trim();
            MarginL = float.Parse(parts[5].Trim());
            MarginR = float.Parse(parts[6].Trim());
            MarginV = float.Parse(parts[7].Trim());
            Effect = parts[8].Trim();

            //we want to treat any remaining as the text, so we join them back together
            text = string.Join(",", parts, 9, parts.Length - 9).Trim();

            //if there is region=XXXX, at the start, remove it
            if (text.StartsWith("region="))
            {
                //go up to where the first comma is
                int firstCommaIndex = text.IndexOf(',');
                if (firstCommaIndex != -1)
                {
                    text = text.Substring(firstCommaIndex + 1).Trim();
                }
                else
                {
                    text = ""; //if there is no comma, just clear the text
                }
            }

            //cleanup any color codes in the text
            const string alphaPattern = @"{\\alpha&H(..)&\\c&H(..)(..)(..)&}";
            const string colorPattern = @"{.?\\c&H(..)(..)(..)&}";

            //detect alpha patterns first
            text = System.Text.RegularExpressions.Regex.Replace(text, alphaPattern, match =>
            {
                //replace with the color code with alfa merged into it
                byte a = byte.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);
                //invert A
                a = (byte)(255 - a); //invert alpha, since 00 is opaque and FF is transparent
                byte b = byte.Parse(match.Groups[2].Value, System.Globalization.NumberStyles.HexNumber);
                byte g = byte.Parse(match.Groups[3].Value, System.Globalization.NumberStyles.HexNumber);
                byte r = byte.Parse(match.Groups[4].Value, System.Globalization.NumberStyles.HexNumber);
                Color color = new Color32(r, g, b, a);
                return "{" + color.ToDMXString() + "}";
            });

            //do the same with color patterns, assuming 255 alpha
            text = System.Text.RegularExpressions.Regex.Replace(text, colorPattern, match =>
            {
                byte b = byte.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);
                byte g = byte.Parse(match.Groups[2].Value, System.Globalization.NumberStyles.HexNumber);
                byte r = byte.Parse(match.Groups[3].Value, System.Globalization.NumberStyles.HexNumber);
                Color color = new Color32(r, g, b, 255); //assume fully opaque
                return "{" + color.ToDMXString() + "}";
            });
        }
    }

    private struct Style
    {
        public int styleIndex;

        public string name;
        public string fontname;
        public float fontSize;
        public Color PrimaryColour;
        public Color SecondaryColour;
        public Color OutlineColour;
        public Color BackColour;
        public bool bold;
        public bool italic;
        public bool underline;
        public bool strikeout;
        public float scaleX;
        public float scaleY;
        public float spacing;
        public float angle;
        public int borderStyle; //probably a enum
        public float outline;
        public float shadow;
        public Alignment alignment; //probably a enum
        public float marginL;
        public float marginR;
        public float marginV;
        public int encoding; //probably a enum

        public enum Alignment
        {
            BottomLeft = 1,
            BottomCenter = 2,
            BottomRight = 3,
            CenterLeft = 4,
            CenterCenter = 5,
            CenterRight = 6,
            TopLeft = 7,
            TopCenter = 8,
            TopRight = 9
        }

        public string ToEncodedString()
        {
            //encode the boolenas into a flag byte
            byte flags = 0;
            if (bold) flags |= 0b00000001;
            if (italic) flags |= 0b00000010;
            if (underline) flags |= 0b00000100;
            if (strikeout) flags |= 0b00001000;
            return $"{(char)styleIndex}|{fontname}|{fontSize}|{PrimaryColour.ToDMXString()}|{SecondaryColour.ToDMXString()}|{OutlineColour.ToDMXString()}|{BackColour.ToDMXString()}|{(char)flags}|{scaleX}|{scaleY}|{spacing}|{angle}|{borderStyle}|{outline}|{shadow}|{(char)alignment}";
        }

        public override string ToString()
        {
            return $@"{name}, {fontname}, {fontSize}, {PrimaryColour}, {SecondaryColour}, {OutlineColour}, {BackColour}, {bold}, {italic}, {underline}, {strikeout}, {scaleX}, {scaleY}, {spacing}, {angle}, {borderStyle}, {outline}, {shadow}, {alignment}, {marginL}, {marginR}, {marginV}, {encoding}";
        }

        public Style(string rawLine, int index)
        {
            styleIndex = index;

            //its assumed the raw line passed in here is with the "Style: " removed
            string[] parts = rawLine.Split(',');

            //now we need to parse everything out
            name = parts[0].Trim();
            fontname = parts[1].Trim();
            fontSize = float.Parse(parts[2].Trim());
            PrimaryColour = ColorFromHex(parts[3].Trim());
            SecondaryColour = ColorFromHex(parts[4].Trim());
            OutlineColour = ColorFromHex(parts[5].Trim());
            BackColour = ColorFromHex(parts[6].Trim());

            bold = parts[7].Trim() == "-1"; //for some reason -1 is used....??? What the hell
            italic = parts[8].Trim() == "-1";
            underline = parts[9].Trim() == "-1";
            strikeout = parts[10].Trim() == "-1";

            scaleX = float.Parse(parts[11].Trim());
            scaleY = float.Parse(parts[12].Trim());
            spacing = float.Parse(parts[13].Trim());
            angle = float.Parse(parts[14].Trim());
            borderStyle = int.Parse(parts[15].Trim());
            outline = float.Parse(parts[16].Trim());
            shadow = float.Parse(parts[17].Trim());
            alignment = (Alignment)int.Parse(parts[18].Trim());
            marginL = float.Parse(parts[19].Trim());
            marginR = float.Parse(parts[20].Trim());
            marginV = float.Parse(parts[21].Trim());
            encoding = int.Parse(parts[22].Trim());
        }
    }
    private static Color ColorFromHex(string hex)
    {
        //remove the &H at the start
        if (hex.StartsWith("&H"))
        {
            hex = hex.Substring(2);
        }

        /*Color values are expressed in hexadecimal BGR format as &HBBGGRR& or ABGR (with alpha channel) as &HAABBGGRR&.
        Transparency (alpha) can be expressed as &HAA&. Note that in the alpha channel, 00 is opaque and FF is transparent. */

        //split into 3 or 4 parts depending on if there is an alpha channel
        if (hex.Length == 6)
        {
            //no alpha channel
            byte b = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte r = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return new Color32(r, g, b, 255); //assume fully opaque
        }
        else if (hex.Length == 8)
        {
            //with alpha channel
            byte a = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            byte r = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            //flip alpha
            a = (byte)(255 - a); //invert alpha, since 00 is opaque and FF is transparent
            return new Color32(r, g, b, a);
        }
        else
        {
            throw new FormatException("Invalid hex color format: " + hex);
        }
    }
}
