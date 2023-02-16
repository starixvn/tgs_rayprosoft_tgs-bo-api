#nullable enable

using System;

namespace TGS.Apis.BackendOffice.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class TgsActionAttribute : Attribute
{
    public string Name { get; }

    public TgsActionAttribute(string name)
    {
        Name = name;
    }
}
