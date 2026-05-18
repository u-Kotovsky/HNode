using System;

/// <summary>
/// Marks alternative tag names (aliases) for a class that is auto-mapped to YAML tags.
/// Multiple aliases can be specified.
/// This attribute is non-inheritable.
/// </summary>
[AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, Inherited = false, AllowMultiple = true)]
public class TagAliasAttribute : System.Attribute
{
    public string Alias { get; }

    public TagAliasAttribute(string alias)
    {
        Alias = alias;
    }
}
