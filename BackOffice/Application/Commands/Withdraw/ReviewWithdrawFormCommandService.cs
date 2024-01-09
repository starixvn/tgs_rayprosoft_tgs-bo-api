using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Commands.Withdraw;

internal class ReviewWithdrawFormCommandService(
    IPaymentServiceClient paymentServiceClient)
    : ICommandService<ReviewWithdrawFormCommand>
{
    public ValueTask ExecuteAsync(
        ReviewWithdrawFormCommand command,
        CancellationToken cancellationToken = default)
        => paymentServiceClient.ReviewWithdrawFormAsync(
            command.FormId,
            command.BrandPaymentId,
            command.Status,
            command.ReviewUserId,
            command.ReviewUserName,
            command.Remark,
            cancellationToken);
}
