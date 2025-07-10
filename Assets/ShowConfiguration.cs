using System.Collections.Generic;
using UnityEngine;
using static ChannelRemapper;
using static UVRemapper;

public class ShowConfiguration
{
    //these are both saved as part of a show configuration, and as part of player prefs
    public IDMXSerializer Serializer { get; set; }
    public IDMXSerializer Deserializer { get; set; }
    public List<IDMXGenerator> Generators { get; set; }
    public bool Transcode { get; set; }
    public int TranscodeUniverseCount { get; set; } = 3;
    public int SerializeUniverseCount { get; set; } = int.MaxValue; //this is the maximum number of universes that can be used for serializing.

    //these features are specifically limited to show configurations since it would be a utter pita to define these via UI alone
    public List<ChannelMapping> mappingsChannels { get; set; }
    public List<UVMapping> mappingsUV { get; set; }
    public List<int> maskedChannels { get; set; }
    /// <summary>
    /// If true, the mask will be inverted, meaning that the channels that channels set in <see cref="maskedChannels"/> will be the only ones visible.
    /// </summary>
    public bool invertMask { get; set; }
    /// <summary>
    /// If true, the mask will automatically be applied to channels that are set to zero.
    /// </summary>
    public bool autoMaskOnZero { get; set; }

    //public string ArtnetIP { get; set; } = "127.0.0.1"; //disabled for now as this is not natively supported by the library yet?
    //public int ArtnetPort { get; set; } = 6454;
    public string SpoutInputName { get; set; } = "HNode Input";
    public string SpoutOutputName { get; set; } = "HNode Output";

    //initializer
    public ShowConfiguration()
    {
        mappingsChannels = new List<ChannelMapping>();
        mappingsUV = new List<UVMapping>();
        maskedChannels = new List<int>();
        Generators = new List<IDMXGenerator>();
    }
}
