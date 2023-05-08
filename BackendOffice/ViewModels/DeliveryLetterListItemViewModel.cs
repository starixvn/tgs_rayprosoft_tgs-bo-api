#nullable enable

using System.Text.Json;
using System;

namespace TGS.Apis.BackendOffice.ViewModels;

public class DeliveryLetterListItemViewModel
{
    public required Guid Id { get; set; }

    public required string Sender { get; set; }

    public required string ReceiverType { get; set; }

    public JsonDocument? Receiver { get; set; }

    public required string Subject { get; set; }

    public DateTime SentOn { get; set; }
}
