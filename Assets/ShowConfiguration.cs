using System.Collections.Generic;
using UnityEngine;
using static ChannelRemapper;
using static UVRemapper;

public class ShowConfiguration
{
    //these are both saved as part of a show configuration, and as part of player prefs
    public IDMXSerializer Serializer { get; set; }
    public IDMXSerializer Deserializer { get; set; }
    public bool Transcode { get; set; }
    public int TranscodeUniverseCount { get; set; }

    //these features are specifically limited to show configurations since it would be a utter pita to define these via UI alone
    public List<ChannelMapping> mappingsChannels { get; set; }
    public List<UVMapping> mappingsUV { get; set; }
    public List<int> maskedChannels { get; set; }
    public bool invertMask { get; set; }

    //initializer
    public ShowConfiguration()
    {
        mappingsChannels = new List<ChannelMapping>();
        mappingsUV = new List<UVMapping>();
        maskedChannels = new List<int>();
    }
}
