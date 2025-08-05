using System;
using UnityEngine;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

/// <summary>
/// Use this type to represent a DMX channel index. Allows for raw index of 0 up, or universe.channel mapping, including equation support
/// </summary>
public struct DMXChannel : IYamlConvertible
{
    private int globalChannel;

    public DMXChannel(int globalChannel)
    {
        this.globalChannel = globalChannel;
    }

    public void Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer)
    {
        var scalar = parser.Consume<Scalar>();
        //convert
        globalChannel = StringToGlobalRepresentation(scalar.Value);
    }

    public void Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
    {
        emitter.Emit(new Scalar(this.ToString()));
    }

    //set operator
    public static implicit operator DMXChannel(int value)
    {
        return new DMXChannel(value);
    }

    //implicit return in form of global channel
    public static implicit operator int(DMXChannel channel)
    {
        return channel.globalChannel;
    }

    //to string, represent in universe.channel format
    public static implicit operator string(DMXChannel channel)
    {
        int universe = (channel.globalChannel / 512) + 1;
        //1 to 512
        int channelInUniverse = channel.globalChannel - ((universe - 1) * 512) + 1;
        return $"{universe}.{channelInUniverse}";
    }

    public override string ToString()
    {
        return (string)this;
    }

    //from string in form of universe.channel
    public static implicit operator DMXChannel(string value)
    {
        int globalChannel = StringToGlobalRepresentation(value);
        return new DMXChannel(globalChannel);
    }

    private static int StringToGlobalRepresentation(string value)
    {
        string[] parts = value.Split('.');
        //check if its global representation
        if (parts.Length == 1)
        {
            if (EquationNumber.TryParse(parts[0], out EquationNumber inglobalChannel))
            {
                if (inglobalChannel < 0)
                {
                    throw new System.ArgumentOutOfRangeException("Global channel must be non-negative.");
                }
                return inglobalChannel;
            }
            else
            {
                throw new System.FormatException("Invalid DMX channel format. Expected format: 'universe.channel' or a non-negative integer for global channel.");
            }
        }
        if (parts.Length != 2)
        {
            throw new System.FormatException("Invalid DMX channel format. Expected format: 'universe.channel'");
        }

        if (!EquationNumber.TryParse(parts[0], out EquationNumber universe) || !EquationNumber.TryParse(parts[1], out EquationNumber channelInUniverse))
        {
            throw new System.FormatException("Invalid DMX channel format. Universe and channel must be integers.");
        }

        if (universe < 1 || universe > 512 || channelInUniverse < 1 || channelInUniverse > 512)
        {
            throw new System.ArgumentOutOfRangeException("Universe and channel must be between 1 and 512.");
        }

        int globalChannel = (universe - 1) * 512 + (channelInUniverse - 1);
        return globalChannel;
    }
}
