#nullable enable

using System;

namespace TGS.Apis.BackendOffice.ViewModels;

public class DeliveryLetterListItemViewModel
{
    public required Guid Id { get; set; }

    public required string Operator { get; set; }

    public required string ReceiverType { get; set; }

    public DateTime SentOn { get; set; }

    public required string Subject { get; set; }
}
