using System.Collections.Generic;
using SFB;
using UnityEngine;

public class TextFileExporter : IExporter
{
    /// <summary>
    /// If true, only channels with non-zero values will be exported.
    /// </summary>
    public bool onlyNonZeroChannels = false;

    private List<byte> data;

    public void CompleteFrame(ref List<byte> channelValues)
    {
        data = channelValues;
    }
    public void Construct() {}
    public void Deconstruct() {}
    public void InitFrame(ref List<byte> channelValues) {}
    public void SerializeChannel(byte channelValue, int channel) {}
    public void ConstructUserInterface(RectTransform rect)
    {
        //toggle for only non-zero channels
        Util.AddToggle(rect, "Only export non-zero channels")
            .WithValue(onlyNonZeroChannels)
            .WithCallback((value) => onlyNonZeroChannels = value);

        //button that when triggered, opens a export prompt and saves a text file with all channels
        Util.AddButton(rect, "Export channels to text file")
            .WithCallback(() =>
            {
                var extensions = new[] {
                    new ExtensionFilter("Channel Information", "chinfo"),
                };
                var path = StandaloneFileBrowser.SaveFilePanel("Save File", "", "channelinfo.chinfo", extensions);

                //create a dictionary between channel index and value
                Dictionary<DMXChannel, byte> channelDict = new Dictionary<DMXChannel, byte>();
                //fill up the dictionary
                foreach (var channel in data)
                {
                    channelDict[(DMXChannel)channelDict.Count] = channel;
                }

                //trim all channels that have a 0 value
                if (onlyNonZeroChannels)
                {
                    var keysToRemove = new List<DMXChannel>();
                    foreach (var key in channelDict.Keys)
                    {
                        if (channelDict[key] == 0)
                        {
                            keysToRemove.Add(key);
                        }
                    }
                    foreach (var key in keysToRemove)
                    {
                        channelDict.Remove(key);
                    }
                }

                //write this to a file, with each channel on a new line in the format "channelIndex: channelValue"
                List<string> lines = new List<string>();
                foreach (var line in channelDict.Keys)
                {
                    lines.Add($"{(string)line}: {channelDict[line]}");
                }

                System.IO.File.WriteAllLines(path, lines);
            });
    }
    public void DeconstructUserInterface() {}
    public void UpdateUserInterface() {}
}
