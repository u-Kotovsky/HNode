using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LRC : Text
{
    //file path to LRC file
    public string filePath = "";

    public Mode mode = Mode.OnConfigLoad;
    /// <summary>
    /// When this channel changes value, the next lyric event will be triggered
    /// </summary>
    public DMXChannel triggerChannel = 0;
    private byte lastTriggerValue = 0;
    private TimeSpan timeAtLoad = TimeSpan.Zero;

    private List<LRCEvent> events = new List<LRCEvent>();
    private int currentEvent;

    public override void GenerateDMX(ref List<byte> dmxData)
    {
        if (events.Count > 0 && currentEvent < events.Count)
        {
            text = events[currentEvent].text;

            //try to move to the next event if needed
            switch (mode)
            {
                case Mode.OnConfigLoad:
                    //move to the next event if the current one has passed
                    if (DateTime.Now.TimeOfDay > timeAtLoad + events[currentEvent].timestamp)
                    {
                        currentEvent++;
                        Debug.Log(events[currentEvent].text);
                    }
                    break;
                case Mode.StrobeTrigger:
                    if (lastTriggerValue != dmxData[triggerChannel])
                    {
                        lastTriggerValue = dmxData[triggerChannel];
                        currentEvent++;
                    }
                    break;
                case Mode.PlayTrigger:
                    if (dmxData[triggerChannel] < 127)
                    {
                        //start playback
                        timeAtLoad = DateTime.Now.TimeOfDay;
                        currentEvent = 0;
                    }
                    else
                    {
                        //move to the next event if the current one has passed
                        if (DateTime.Now.TimeOfDay > timeAtLoad + events[currentEvent].timestamp)
                        {
                            currentEvent++;
                            Debug.Log(events[currentEvent].text);
                        }
                    }
                    break;
            }
        }

        //call base now that we have filled the text
        base.GenerateDMX(ref dmxData);
    }

    //constructor, unusual but used to setup the initial file load
    public override void Construct()
    {
        Debug.Log("LRC Construct called, loading file: " + filePath);
        string lyricsraw = "";
        //load the file if it exists
        if (System.IO.File.Exists(filePath))
        {
            lyricsraw = System.IO.File.ReadAllText(filePath);
        }
        else
        {
            Debug.LogWarning("LRC file not found: " + filePath);
            lyricsraw = "";
        }

        //split based on line
        string[] lines = lyricsraw.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        foreach (string line in lines)
        {
            //we need to extract all possible timestamps on a line

            //a LRC event line can have multiple timestamps in it, like this
            //[00:12.00][00:17.20][00:20.00]Line of lyrics here

            //find all of them using regex
            System.Text.RegularExpressions.MatchCollection matches = System.Text.RegularExpressions.Regex.Matches(line, @"\[(\d{2}):(\d{2})(?:\.(\d{2}))?\]");
            //remove all the matches from the string
            string lyricText = System.Text.RegularExpressions.Regex.Replace(line, @"\[(\d{2}):(\d{2})(?:\.(\d{2}))?\]", "").Trim();
            if (matches.Count > 0)
            {
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    //extract the timestamp
                    int minutes = int.Parse(match.Groups[1].Value);
                    int seconds = int.Parse(match.Groups[2].Value);
                    int milliseconds = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;

                    //create a new LRC event
                    LRCEvent lrcEvent = new LRCEvent
                    {
                        timestamp = new TimeSpan(0, 0, minutes, seconds, milliseconds * 10),
                        text = lyricText
                    };

                    //add it to the list
                    events.Add(lrcEvent);
                }
            }
        }

        //print out all the lyrics
        /* foreach (LRCEvent lrcEvent in events)
        {
            Debug.Log($"[{lrcEvent.timestamp}] {lrcEvent.text}");
        } */

        timeAtLoad = DateTime.Now.TimeOfDay;
    }

    public enum Mode
    {
        /// <summary>
        /// Will wait for a trigger channel to go to the next lyric event
        /// </summary>
        StrobeTrigger,
        /// <summary>
        /// Will wait for a trigger channel to start timecoded playback
        /// </summary>
        PlayTrigger,
        /// <summary>
        /// Will play the LRC file using timestamps the second the config is loaded
        /// </summary>
        OnConfigLoad
    }

    private struct LRCEvent
    {
        public TimeSpan timestamp;
        public string text;
    }
}
