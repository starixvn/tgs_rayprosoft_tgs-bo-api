#nullable enable

using System;
using TGS.Services.BackOffice.Domain.Enums;

namespace TGS.Apis.BackendOffice.ViewModels;

public class AccountChangesQueryViewModel
{
    public TransactionType? AccountChangeType { get; set; }

    public DateTime From { get; set; }

    public DateTime To { get; set; }
}
