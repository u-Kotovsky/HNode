using System;

/// <summary>
/// Marks a class or struct to be automatically mapped to a YAML tag based on its name.
/// </summary>
[AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct | AttributeTargets.Interface, Inherited = true)]
public class TagMappedAttribute : System.Attribute { }
