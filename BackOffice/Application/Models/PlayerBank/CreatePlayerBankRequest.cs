#nullable enable

using System;
using System.Text.Json;

namespace TGS.Services.BackOffice.Application.Models.PlayerBank;

public class CreatePlayerBankRequest
{
    public string BankType { get; init; } = default!;

    public Guid BrandId { get; init; } = default!;

    public JsonDocument Detail { get; init; } = default!;

    public Guid PaymentChannelId { get; init; } = default!;
}
