using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SRT : Text
{
    //file path to SRT file
    public string filePath = "";

    public Mode mode = Mode.OnConfigLoad;
    /// <summary>
    /// When this channel changes value, the next lyric event will be triggered
    /// </summary>
    public int triggerChannel = 0;
    private byte lastTriggerValue = 0;
    private TimeSpan timeAtLoad = TimeSpan.Zero;

    private List<SRTEvent> events = new List<SRTEvent>();
    private int currentEvent;

    public override void GenerateDMX(ref List<byte> dmxData)
    {
        //clear text
        text = "";

        if (events.Count != 0 && currentEvent < events.Count)
        {
            //try to move to the next event if needed
            switch (mode)
            {
                case Mode.OnConfigLoad:
                    //move to the next event if the current one has passed
                    if (DateTime.Now.TimeOfDay > timeAtLoad + events[currentEvent].start &&
                        DateTime.Now.TimeOfDay < timeAtLoad + events[currentEvent].end)
                    {
                        text = events[currentEvent].text;
                        Debug.Log(events[currentEvent].text);
                    }

                    if (DateTime.Now.TimeOfDay > timeAtLoad + events[currentEvent].end)
                    {
                        //keep going till we find a fresh one, since there might be multiple events with the same times
                        while (currentEvent < events.Count - 1 && DateTime.Now.TimeOfDay > timeAtLoad + events[currentEvent + 1].start)
                        {
                            currentEvent++;
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
        Debug.Log("SRT Construct called, loading file: " + filePath);
        string lyricsraw = "";
        //load the file if it exists
        if (System.IO.File.Exists(filePath))
        {
            lyricsraw = System.IO.File.ReadAllText(filePath);
        }
        else
        {
            Debug.LogWarning("SRT file not found: " + filePath);
            lyricsraw = "";
        }

        //add some new lines to make sure the regex works correctly
        lyricsraw += "\n\n\n";

        //find all of them using regex
        const string pattern = @"(?m)^\d+\n*((\d{2}):(\d{2}):(\d{2}),(\d{3})) --> ((\d{2}):(\d{2}):(\d{2}),(\d{3}))\n*([\S\s]*?(?=\n\n))";
        System.Text.RegularExpressions.MatchCollection matches = System.Text.RegularExpressions.Regex.Matches(lyricsraw, pattern);

        if (matches.Count > 0)
        {
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                //extract the timestamp
                int hoursStart = int.Parse(match.Groups[2].Value);
                int minutesStart = int.Parse(match.Groups[3].Value);
                int secondsStart = int.Parse(match.Groups[4].Value);
                int millisecondsStart = match.Groups[5].Success ? int.Parse(match.Groups[3].Value) : 0;
                int hoursEnd = int.Parse(match.Groups[7].Value);
                int minutesEnd = int.Parse(match.Groups[8].Value);
                int secondsEnd = int.Parse(match.Groups[9].Value);
                int millisecondsEnd = match.Groups[10].Success ? int.Parse(match.Groups[9].Value) : 0;

                //create a new LRC event
                SRTEvent lrcEvent = new SRTEvent
                {
                    start = new TimeSpan(0, hoursStart, minutesStart, secondsStart, millisecondsStart),
                    end = new TimeSpan(0, hoursEnd, minutesEnd, secondsEnd, millisecondsEnd),
                    text = match.Groups[11].Value.Trim()
                };

                //add it to the list
                events.Add(lrcEvent);
            }
        }

        //print out all the lyrics
        /* foreach (LRCEvent lrcEvent in events)
        {
            Debug.Log($"[{lrcEvent.timestamp}] {lrcEvent.text}");
        } */

        Debug.Log($"Loaded {events.Count} SRT events from file: {filePath}");

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

    private struct SRTEvent
    {
        public TimeSpan start;
        public TimeSpan end;
        public string text;
    }
}
