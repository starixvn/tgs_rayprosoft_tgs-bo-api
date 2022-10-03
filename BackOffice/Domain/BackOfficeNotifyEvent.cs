using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.Json;

#nullable enable

namespace TGS.Services.BackOffice.Domain;
public record BackOfficeNotifyEvent
{
    private readonly NameValueCollection _tags = new();

    public JsonDocument Body { get; private set; }

    public BackOfficeNotifyEvent(IEnumerable<KeyValuePair<string, string>> tags, string body)
    {
        foreach (var tag in tags)
            _tags.Add(tag.Key, tag.Value);

        Body = JsonDocument.Parse(body);
    }

    public string? this[string name] => GetTagValues(name)?[^1];

    public string[]? GetTagValues(string name) => _tags.GetValues(name);

    public bool Match(string name, string value)
        => GetTagValues(name)
            ?.Any(v => v.Equals(value, System.StringComparison.OrdinalIgnoreCase)) ?? false;
}
