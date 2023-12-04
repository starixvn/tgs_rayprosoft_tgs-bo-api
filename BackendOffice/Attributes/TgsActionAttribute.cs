#nullable enable

using System;

namespace TGS.Apis.BackendOffice.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class TgsActionAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
