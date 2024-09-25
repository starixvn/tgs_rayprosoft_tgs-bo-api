using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.Clients;
using Tgs.Payment.ManualDeposit;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Controllers.Messages.ManualDeposit;
using TGS.Payment.ManualDeposit;
using TGS.Services.BackOffice.ApplicationServices.Payment;

#nullable enable

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ManualDepositController : BaseContextController
{
    [HttpPut("bank")]
    public async ValueTask<Guid> SubmitDepositBankRequestAsync(
    [FromServices] IGetService<CreateDepositFormCommand, Guid> createDepositFormService,
    [FromServices] IGetService<GetBrandPaymentQuery, BrandPaymentInfo> getBrandPaymentService,
    [FromServices] ManualDepositSerializer serializer,
    [FromBody] SubmitManualDepositBankRequest request)
    {
        var brandPayment = await getBrandPaymentService
            .GetAsync(new(request.BrandPaymentId), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var brandPaymentBankInfo = JsonDocument.Parse(brandPayment.ExtendedData!);

        var command = new CreateDepositFormCommand(
            request.PlayerCode,
            request.BrandId,
            request.BrandPaymentId,
            request.Amount,
            "tgs.payment.manual.deposit",
            request.PromotionId,
            _ => serializer.Serialize(
                new ManualDepositBankApplyInfo()
                {
                    BankType = request.BankType,
                    SendBankAccount = request.BankAccount,
                    SendBankName = request.BankName,
                    SendNote = request.Note,
                    SendName = request.RealName,
                    ReceiveBankAccount = brandPaymentBankInfo.RootElement.GetProperty("bankAccount").GetString()!,
                    ReceiveAccountName = brandPaymentBankInfo.RootElement.GetProperty("name").GetString()!
                }));

        return await createDepositFormService
            .GetAsync(command, HttpContext.RequestAborted)
            .ConfigureAwait(false);
    }

    [HttpPut("ewallet", Name = "SubmitDepositEWalletRequest")]
    public async ValueTask<Guid> SubmitDepositEWalletRequestAsync(
       [FromServices] IGetService<CreateDepositFormCommand, Guid> createDepositFormService,
       [FromServices] IGetService<GetBrandPaymentQuery, BrandPaymentInfo> getBrandPaymentService,
       [FromServices] ManualDepositSerializer serializer,
       [FromBody] SubmitManualDepositEWalletRequest request)
    {
        var brandPayment = await getBrandPaymentService
            .GetAsync(new(request.BrandPaymentId), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var brandPaymentBankInfo = JsonDocument.Parse(brandPayment.ExtendedData!);

        string? qrCode = null;

        if (brandPaymentBankInfo.RootElement.TryGetProperty("qrCode", out var jsonElementQrCode))
            qrCode = jsonElementQrCode.GetString();

        var command = new CreateDepositFormCommand(
            request.PlayerCode,
            request.BrandId,
            request.BrandPaymentId,
            request.Amount,
            "tgs.payment.manual.deposit",
            request.PromotionId,
            _ => serializer.Serialize(
                new ManualDepositEWalletApplyInfo
                {
                    EMoneyName = brandPaymentBankInfo.RootElement.GetProperty("eMoneyName").GetString()!,
                    Phone = brandPaymentBankInfo.RootElement.GetProperty("phone").GetString()!,
                    QrCode = qrCode,
                    SendNote = request.Note,
                    BankType = request.BankType,
                    SendEMoneyName = request.RealName,
                    SendPhone = request.Phone,
                }));

        return await createDepositFormService
            .GetAsync(command, HttpContext.RequestAborted)
            .ConfigureAwait(false);
    }

    [HttpPut("fps", Name = "SubmitDepositFpsRequest")]
    public async ValueTask<Guid> SubmitDepositFpsRequestAsync(
       [FromServices] IGetService<CreateDepositFormCommand, Guid> createDepositFormService,
       [FromServices] IGetService<GetBrandPaymentQuery, BrandPaymentInfo> getBrandPaymentService,
       [FromServices] ManualDepositSerializer serializer,
       [FromBody] SubmitManualDepositFpsRequest request)
    {
        var brandPayment = await getBrandPaymentService
            .GetAsync(new(request.BrandPaymentId), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var brandPaymentBankInfo = JsonDocument.Parse(brandPayment.ExtendedData!);

        string? qrCodeFileUrl = null;

        if (brandPaymentBankInfo.RootElement.TryGetProperty("qrCodeFileUrl", out var jsonElementQrCodeFileUrl))
            qrCodeFileUrl = jsonElementQrCodeFileUrl.GetString();

        string? email = null;

        if (brandPaymentBankInfo.RootElement.TryGetProperty("email", out var jsonElementEmail))
            email = jsonElementEmail.GetString();

        string? fpsId = null;

        if (brandPaymentBankInfo.RootElement.TryGetProperty("fpsId", out var jsonElementFpsId))
            fpsId = jsonElementFpsId.GetString();

        string? phone = null;

        if (brandPaymentBankInfo.RootElement.TryGetProperty("phone", out var jsonElementPhone))
            phone = jsonElementPhone.GetString();

        var command = new CreateDepositFormCommand(
            request.PlayerCode,
            request.BrandId,
            request.BrandPaymentId,
            request.Amount,
            "tgs.payment.manual.deposit",
            request.PromotionId,
            _ => serializer.Serialize(
                new ManualDepositFpsApplyInfo
                {
                    SendBankAccount = request.BankAccount,
                    SendBankName = request.BankName,
                    BankType = request.BankType,
                    QrCodeFileUrl = qrCodeFileUrl,
                    Email = email,
                    FpsId = fpsId,
                    Phone = phone
                }));

        return await createDepositFormService
            .GetAsync(command, HttpContext.RequestAborted)
            .ConfigureAwait(false);
    }

    [HttpPut("pulsa", Name = "SubmitDepositPulsaRequest")]
    public async ValueTask<Guid> SubmitDepositPulsaRequestAsync(
       [FromServices] IGetService<CreateDepositFormCommand, Guid> createDepositFormService,
       [FromServices] IGetService<GetBrandPaymentQuery, BrandPaymentInfo> getBrandPaymentService,
       [FromServices] ManualDepositSerializer serializer,
       [FromBody] SubmitManualDepositPulsaRequest request)
    {
        var brandPayment = await getBrandPaymentService
            .GetAsync(new(request.BrandPaymentId), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var brandPaymentBankInfo = JsonDocument.Parse(brandPayment.ExtendedData!);

        var command = new CreateDepositFormCommand(
            request.PlayerCode,
            request.BrandId,
            request.BrandPaymentId,
            request.Amount,
            "tgs.payment.manual.deposit",
            request.PromotionId,
            _ => serializer.Serialize(
                new ManualDepositPulsaApplyInfo
                {
                    Phone = brandPaymentBankInfo.RootElement.GetProperty("phone").GetString()!,
                    SendNote = request.Note,
                    BankType = request.BankType,
                    SendName = request.RealName,
                    SendPhone = request.Phone
                }));

        return await createDepositFormService
            .GetAsync(command, HttpContext.RequestAborted)
            .ConfigureAwait(false);
    }

    [HttpPut("thirdparty")]
    public async ValueTask<Guid> SubmitDepositThirdPartyRequestAsync(
        [FromServices] IGetService<CreateDepositFormCommand, Guid> createDepositFormService,
        [FromServices] IGetService<GetBrandPaymentQuery, BrandPaymentInfo> getBrandPaymentService,
        [FromServices] ManualDepositSerializer serializer,
        [FromBody] SubmitManualDepositThirdPartyRequest request)
    {
        var brandPayment = await getBrandPaymentService
            .GetAsync(new(request.BrandPaymentId), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var brandPaymentBankInfo = JsonDocument.Parse(brandPayment.ExtendedData!);

        var command = new CreateDepositFormCommand(
            request.PlayerCode,
            request.BrandId,
            request.BrandPaymentId,
            request.Amount,
            "tgs.payment.manual.deposit",
            request.PromotionId,
            _ => serializer.Serialize(
                new ManualDepositThirdPartyApplyInfo
                {
                    BankType = request.BankType,
                    QrCode = brandPaymentBankInfo.RootElement.GetProperty("qrCode").GetString()!
                }));

        return await createDepositFormService
            .GetAsync(command, HttpContext.RequestAborted)
            .ConfigureAwait(false);
    }

    [HttpPut("usdt", Name = "SubmitDepositUsdtRequest")]
    public async ValueTask<Guid> SubmitDepositUsdtRequestAsync(
       [FromServices] IGetService<CreateDepositFormCommand, Guid> createDepositFormService,
       [FromServices] IGetService<GetBrandPaymentQuery, BrandPaymentInfo> getBrandPaymentService,
       [FromServices] ManualDepositSerializer serializer,
       [FromBody] SubmitManualDepositUsdtRequest request)
    {
        var brandPayment = await getBrandPaymentService
            .GetAsync(new(request.BrandPaymentId), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var brandPaymentBankInfo = JsonDocument.Parse(brandPayment.ExtendedData!);

        var rate = brandPaymentBankInfo.RootElement.GetProperty("rate").GetDecimal();

        var command = new CreateDepositFormCommand(
            request.PlayerCode,
            request.BrandId,
            request.BrandPaymentId,
            request.Amount / rate,
            "tgs.payment.manual.deposit",
            request.PromotionId,
            _ => serializer.Serialize(
                new ManualDepositUsdtApplyInfo
                {
                    BankType = request.BankType,
                    Address = brandPaymentBankInfo.RootElement.GetProperty("address").GetString()!,
                    QrCode = brandPaymentBankInfo.RootElement.GetProperty("qrCode").GetString(),
                    Rate = rate,
                    SendAmount = request.Amount
                }));

        return await createDepositFormService
            .GetAsync(command, HttpContext.RequestAborted)
            .ConfigureAwait(false);
    }
}
