#nullable enable

using System.Text.Json;
using System;

namespace TGS.Apis.BackendOffice.ViewModels;

public class PlayerBankInfoViewModel
{
    public required Guid Id { get; set; }

    public required Guid PaymentChannelId { get; set; }

    public required string BankType { get; set; }

    public required JsonDocument Detail { get; set; }

    public required bool IsDefault { get; set; }

    public required DateTime UpdatedOn { get; set; }

    public string? Modifier { get; set; }
}
