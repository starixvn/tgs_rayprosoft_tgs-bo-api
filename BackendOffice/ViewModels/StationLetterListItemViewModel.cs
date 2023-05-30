#nullable enable

using System;

namespace TGS.Apis.BackendOffice.ViewModels;

public class StationLetterListItemViewModel
{
    public Guid Id { get; set; }

    public bool Replied { get; set; }

    public required string Sender { get; set; }

    public DateTime SentOn { get; set; }

    public required string Subject { get; set; }
}
