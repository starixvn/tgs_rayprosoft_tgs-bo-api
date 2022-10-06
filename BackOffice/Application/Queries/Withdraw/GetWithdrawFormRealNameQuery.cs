namespace TGS.Services.BackOffice.Application.Queries.Withdraw
{
    public record GetWithdrawFormRealNameQuery(Tgs.Payment.Clients.UserType UserType, string UserId);
}
