using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SRT : Text
{
    //file path to SRT file
    public string filePath = "";
    public bool generateSubtitlePercentage= false;
    public int subtitlePercentChannel = 0;

    public Mode mode = Mode.OnConfigLoad;
    private TimeSpan timeAtLoad = TimeSpan.Zero;

    private List<SRTEvent> events = new List<SRTEvent>();
    private int currentEvent;

    public override void GenerateDMX(ref List<byte> dmxData)
    {
        //clear text
        text = "";

        if (events.Count != 0 && currentEvent < events.Count)
        {
            //figure out the percentage of the current event
            float percentage = 0f;
            if (events[currentEvent].end != events[currentEvent].start)
            {
                percentage = (float)(DateTime.Now.TimeOfDay - timeAtLoad - events[currentEvent].start).TotalMilliseconds /
                             (float)(events[currentEvent].end - events[currentEvent].start).TotalMilliseconds;
            }
            //convert to a value between 0 and 255
            int percentageValue = Mathf.Clamp(Mathf.RoundToInt(percentage * 255f), 0, 255);
            //set the percentage channel
            if (generateSubtitlePercentage)
            {
                //make sure we can write to that by expanding it first
                dmxData.EnsureCapacity(subtitlePercentChannel + 1);
                dmxData[subtitlePercentChannel] = (byte)percentageValue;
            }

            //try to move to the next event if needed
            switch (mode)
            {
                case Mode.OnConfigLoad:
                    //move to the next event if the current one has passed
                    if (DateTime.Now.TimeOfDay > timeAtLoad + events[currentEvent].start &&
                        DateTime.Now.TimeOfDay < timeAtLoad + events[currentEvent].end)
                    {
                        text = events[currentEvent].text;
                    }

                    if (DateTime.Now.TimeOfDay > timeAtLoad + events[currentEvent].end)
                    {
                        Debug.Log(events[currentEvent]);
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
                int millisecondsStart = int.Parse(match.Groups[5].Value);
                int hoursEnd = int.Parse(match.Groups[7].Value);
                int minutesEnd = int.Parse(match.Groups[8].Value);
                int secondsEnd = int.Parse(match.Groups[9].Value);
                int millisecondsEnd = int.Parse(match.Groups[10].Value);

                //Debug.Log($"Found SRT event: {hoursStart}:{minutesStart}:{secondsStart}.{millisecondsStart} --> {hoursEnd}:{minutesEnd}:{secondsEnd}.{millisecondsEnd} - {match.Groups[11].Value.Trim()}");

                //create a new LRC event
                SRTEvent srtEvent = new SRTEvent
                {
                    start = new TimeSpan(0, hoursStart, minutesStart, secondsStart, millisecondsStart),
                    end = new TimeSpan(0, hoursEnd, minutesEnd, secondsEnd, millisecondsEnd),
                    text = match.Groups[11].Value.Trim()
                };

                //add it to the list
                events.Add(srtEvent);
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
        OnConfigLoad
    }

    private struct SRTEvent
    {
        public TimeSpan start;
        public TimeSpan end;
        public string text;

        public override string ToString()
        {
            return $"{start} --> {end}: {text}";
        }
    }
}
