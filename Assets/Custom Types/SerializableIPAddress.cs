
//alias to integer, allows for math like (5 * 2) + 3 to be converted to a integer dynamically
using System;
using System.Data;
using System.Net;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

public struct SerializableIPAddress : IYamlConvertible
{
    private IPAddress _value;

    public void Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer)
    {
        var scalar = parser.Consume<Scalar>();
        //parse to a string
        _value = IPAddress.Parse(scalar.Value);
    }

    public void Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
    {
        //emit the equation back
        emitter.Emit(new Scalar(_value.ToString()));
    }

    //conversion from IPAddress to SerializableIPAddress
    public static implicit operator SerializableIPAddress(IPAddress address)
    {
        return new SerializableIPAddress { _value = address };
    }

    //conversion from SerializableIPAddress to IPAddress
    public static implicit operator IPAddress(SerializableIPAddress address)
    {
        return address._value;
    }

    //conversion from string
    public static implicit operator SerializableIPAddress(string address)
    {
        return new SerializableIPAddress { _value = IPAddress.Parse(address) };
    }

    //implicit to string
    public static implicit operator string(SerializableIPAddress address)
    {
        return address._value.ToString();
    }

    public override string ToString()
    {
        return _value.ToString();
    }
}
