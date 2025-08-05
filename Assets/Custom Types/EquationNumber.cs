
//alias to integer, allows for math like (5 * 2) + 3 to be converted to a integer dynamically
using System;
using System.Data;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

/// <summary>
/// Use this type to represent equations that will be usable as normal integers
/// </summary>
public struct EquationNumber : IYamlConvertible
{
    private int _value;
    private string _equation;

    public void Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer)
    {
        var scalar = parser.Consume<Scalar>();
        //convert the equation to a number
        _value = Convert(scalar.Value);
        _equation = scalar.Value;
    }

    public void Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
    {
        //emit the equation back
        emitter.Emit(new Scalar(_equation));
    }

    //implicit for string and integers
    public static implicit operator EquationNumber(string equation)
    {
        return new EquationNumber { _equation = equation, _value = Convert(equation) };
    }

    public static implicit operator EquationNumber(int value)
    {
        return new EquationNumber { _value = value, _equation = value.ToString() };
    }

    public static implicit operator int(EquationNumber equationNumber)
    {
        return equationNumber._value;
    }

    public static implicit operator string(EquationNumber equationNumber)
    {
        return equationNumber._equation;
    }

    //add operator
    public static EquationNumber operator +(EquationNumber a, EquationNumber b)
    {
        return new EquationNumber { _value = a._value + b._value, _equation = $"{a._equation} + {b._equation}" };
    }

    //tostring
    public override string ToString()
    {
        return _equation;
    }

    private static int Convert(string equation)
    {
        DataTable dt = new DataTable();
        var val = dt.Compute(equation, "");

        int valu = Int32.Parse(val.ToString());
        return valu;
    }

    public static bool TryParse(string value, out EquationNumber equationNumber)
    {
        try
        {
            equationNumber = new EquationNumber { _equation = value, _value = Convert(value) };
            return true;
        }
        catch
        {
            equationNumber = default;
            return false;
        }
    }
}
