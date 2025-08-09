using YamlDotNet.Serialization;

public struct DMXChannelRange
{
    public DMXChannel start;
    public DMXChannel end;

    public bool Contains(DMXChannel channel)
    {
        return channel >= start && channel <= end;
    }
}
