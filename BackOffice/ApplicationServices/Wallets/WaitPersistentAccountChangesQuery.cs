#nullable enable

using Tgs.CashService.Client;

namespace TGS.Services.BackOffice.ApplicationServices.Wallets;
public record WaitPersistentAccountChangesQuery(
    string PlayerCode,
    PagingConditions Paging);
