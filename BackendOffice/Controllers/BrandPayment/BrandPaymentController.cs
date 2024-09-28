using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tgs.Logging;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Controllers.Messages;
using TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;
using TGS.Apis.BackendOffice.DataService.Services;
using TGS.Apis.BackendOffice.Services;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.ApplicationServices.Payment;
using TGS.Services.BackOffice.Domain;
using Player = TGS.Core.DataServices.Tgs.Models.Player;

namespace TGS.Apis.BackendOffice.Controllers.BrandPayment;

[Route("api/[controller]")]
[ApiController]
public partial class BrandPaymentController(
    ITgsQueries<Player> playerQueries,
    IUploadService uploadService,
    IBrandPaymentService brandPaymentService,
    ITgsQueries<BrandPaymentAuditLog> auditLogQueries,
    ITgsLog tgsLog) : BaseContextController
{
    [HttpPut("ChangeOrders")]
    [Description("Change Payment Order")]
    public async Task<MessageResponse> ChangeOrderAsync(ChangeOrderRequest request)
    {
        await brandPaymentService.ChangeOrderAsync(request);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpPost("deposit")]
    [HttpPost()]
    [Description("Create a Deposit Payment")]
    public async Task<CreateUpdateBrandPaymentResponse> CreateDepositBrandPaymentAsync(CreateBrandPaymentRequest request)
    {
        return await CreateBrandPaymentAsync(PaymentTransferType.Deposit, request);
    }

    [HttpPost("withdraw")]
    [Description("Create a Withdraw Payment")]
    public async Task<CreateUpdateBrandPaymentResponse> CreateWithdrawBrandPaymentAsync(CreateBrandPaymentRequest request)
    {
        return await CreateBrandPaymentAsync(PaymentTransferType.Withdrawal, request);
    }

    [HttpGet("autowithdraw/{brandId:guid}", Name = "GetBrandAutoWithdrawSetting")]
    [Description("Get a brand auto withdraw setting")]
    public ValueTask<AutoWithdrawInfo> GetBrandAutoWithdrawSettingAsync(
        Guid brandId,
        [FromServices] IPaymentServiceClient paymentServiceClient)
        => paymentServiceClient.GetBrandAutoWithdrawSettingAsync(brandId, HttpContext.RequestAborted);

    [HttpGet("info/{id}")]
    [Description("Get brand payment detail info")]
    public async Task<BrandPaymentInfoModel> GetBrandPaymentInfoAsync(
        Guid id,
        [FromServices] IGetService<GetBrandPaymentQuery, BrandPaymentInfo> getBrandPaymentService,
        [FromServices] IPaymentServiceClient paymentServiceClient)
    {
        var info = await getBrandPaymentService
            .GetAsync(new GetBrandPaymentQuery(id), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var brandPaymentVipLevels = await paymentServiceClient
                .GetBrandPaymentVipsAsync([id], HttpContext.RequestAborted)
                .ToArrayAsync()
                .ConfigureAwait(false);

        var brandPaymentFeeInfo = await paymentServiceClient
            .GetBrandPaymentFeeSettingsAsync(id, HttpContext.RequestAborted)
            .ConfigureAwait(false);
        
        var brandpaymentPlayerAccess = await paymentServiceClient
            .GetBrandPaymentPlayerAccessAsync(id, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var adminLogMessage = $"User: {Username} | Action: Read brand payment extended data";
        LogAdminBrandpaymentSettingAccess(id, adminLogMessage);
        return new BrandPaymentInfoModel
        {
            PaymentId = info.PaymentId,
            PaymentMethodId = info.MethodId,
            PaymentChannelId = info.ChannelId,
            PaymentMethodChannelId = info.PaymentMethodChannelId,
            ChannelName = info.ChannelName,
            ExtendedData = JsonDocument.Parse(info.ExtendedData, new JsonDocumentOptions()),
            HandlingFeeRatePercentile = brandPaymentFeeInfo.HandlingFeeRateTenPercentile * 0.1M,
            ReducePlayerAmount = brandPaymentFeeInfo.ReducePlayerAmount,
            PaymentStatus = info.Enabled == true
                ? SwitchStatus.Enabled
                : SwitchStatus.Disabled,
            TransferLimitFrom = info.TransferLimit.From,
            TransferLimitTo = info.TransferLimit.To,
            PlayerListCondition = info.ConditionMatchType == ConditionMatchType.MatchAll
                ? PlayerListCondition.MatchAll
                : PlayerListCondition.MatchOne,
            AllPlayers = info.IsAllPlayers,
            DepositedAmountMin = info.DepositedAmountLimit.From,
            DepositedAmountMax = info.DepositedAmountLimit.To,
            DepositedAmountUnlimited = info.DepositedAmountLimit.To == decimal.MaxValue,
            PhoneAuthentication = info.AuthenticatePhone,
            IdentityAuthentication = info.AuthenticateIdentity,
            VipLevelIds = brandPaymentVipLevels.Select(x => x.VipLevelId).ToArray(),
            TransferType = info.TransferType == TransferType.Deposit
                ? PaymentTransferType.Deposit
                : PaymentTransferType.Withdrawal,
            Blacklist = brandpaymentPlayerAccess.Blacklist.ToArray(),
            Whitelist = brandpaymentPlayerAccess.Whitelist.ToArray()
        };
    }

    [HttpGet]
    [Description("Get brand payments")]
    public async Task<PagedResponse<BrandPaymentModel>> GetBrandPaymentsAsync(
        [FromQuery] GetBrandPaymentsRequest request,
        [FromServices] IPaymentServiceClient paymentServiceClient)
    {
        TransferType? transferType = request.TransferType == null
            ? null
            : request.TransferType == PaymentTransferType.Deposit
                ? TransferType.Deposit
                : TransferType.Withdraw;
        bool? enabled = request.PaymentStatus == null
            ? null
            : request.PaymentStatus == SwitchStatus.Enabled;

        var brandPayments = paymentServiceClient
            .QueryBrandPaymentsAsync(request.BrandId, transferType, enabled, request.Keyword, HttpContext.RequestAborted)
            .ToEnumerable();

        var brandPaymentIds = brandPayments.Select(bp => bp.Id);

        var payments = await paymentServiceClient.GetAllPaymentsAsync(HttpContext.RequestAborted)
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var paymentMethods = await paymentServiceClient.GetAllPaymentMethodsAsync(HttpContext.RequestAborted)
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var brandPaymentVipLevels = await paymentServiceClient.GetBrandPaymentVipsAsync(brandPaymentIds)
            .ToLookupAsync(bv => bv.BrandPaymentId, bv => bv.VipLevelId, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var result = brandPayments
                    .Select(x => new BrandPaymentModel
                    {
                        Id = x.Id,
                        Number = x.DisplayOrder,
                        BrandId = x.BrandId,
                        PaymentId = x.PaymentId,
                        PaymentName = payments.FirstOrDefault(p => p.Id == x.PaymentId) != null
                            ? payments.FirstOrDefault(p => p.Id == x.PaymentId).Code
                            : string.Empty,
                        PaymentMethodId = x.MethodId,
                        PaymentMethodName = paymentMethods.FirstOrDefault(pm => pm.Id == x.MethodId) != null
                            ? paymentMethods.FirstOrDefault(pm => pm.Id == x.MethodId).Name
                            : string.Empty,
                        PaymentChannelId = x.ChannelId,
                        PaymentChannelName = x.ChannelName,
                        PaymentMethodChannelId = x.PaymentMethodChannelId,
                        TransferType = x.TransferType == TransferType.Deposit ? PaymentTransferType.Deposit : PaymentTransferType.Withdrawal,
                        ChannelName = x.ChannelName,
                        ExtendedData = JsonDocument.Parse(x.ExtendedData, new JsonDocumentOptions()),
                        PlayerListCondition = x.ConditionMatchType == ConditionMatchType.MatchAll ? PlayerListCondition.MatchAll : PlayerListCondition.MatchOne,
                        AllPlayers = x.IsAllPlayers,
                        DepositedAmountMin = x.DepositedAmountLimit.From,
                        DepositedAmountMax = x.DepositedAmountLimit.To,
                        DepositedAmountUnlimited = x.DepositedAmountLimit.To == decimal.MaxValue,
                        IdentityAuthentication = x.AuthenticateIdentity,
                        PhoneAuthentication = x.AuthenticatePhone,
                        TransferLimitFrom = x.TransferLimit.From,
                        TransferLimitTo = x.TransferLimit.To,
                        PaymentStatus = x.Enabled ? SwitchStatus.Enabled : SwitchStatus.Disabled,
                        DesignatedUsers = false,
                        VipLevelIds = brandPaymentVipLevels[x.Id].ToArray(),
                        BankType = paymentMethods.FirstOrDefault(pm => pm.Id == x.MethodId) != null
                            ? paymentMethods.FirstOrDefault(pm => pm.Id == x.MethodId).BankType
                            : string.Empty
                    });

        result = result
            .OrderBy(bp => bp.TransferType)
            .ThenBy(bp=>bp.PaymentName)
            .ThenBy(bp=>bp.PaymentMethodName)
            .ThenBy(bp=>bp.ChannelName);

        if (request.ShowAll || request.Rows == 0)
        {
            var allResult = result.ToArray();
            return new PagedResponse<BrandPaymentModel>() { Valid = true, TotalCount = allResult.Length, Items = allResult };
        }

        var totalCount = result.Count();
        var page = request.Page <= 0 ? 0 : request.Page - 1;
        var pageSize = request.Rows <= 0 ? 10 : request.Rows;
        var items = result.Skip(page * pageSize).Take(pageSize).ToArray();

        return new PagedResponse<BrandPaymentModel>() { Valid = true, TotalCount = totalCount, Items = items };
    }

    [HttpGet("{id}/DesignatedUsers")]
    [Description("Get Designated Users")]
    public async Task<GetDesignatedUsersResponse> GetDesignatedUsersAsync(
        Guid id,
        [FromServices] IPaymentServiceClient paymentServiceClient)
    {
        var brandPaymentPlayerInfos = await paymentServiceClient
            .GetBrandPaymentDesignatedUsersAsync(id, HttpContext.RequestAborted)
            .ToArrayAsync()
            .ConfigureAwait(false);

        if (brandPaymentPlayerInfos == null)
        {
            return CreateInvalidResponse<GetDesignatedUsersResponse>(Resources.BrandPayment.NotFound);
        }

        var players = brandPaymentPlayerInfos.Select(bp => bp.Username).ToArray();

        return new() { Valid = true, Players = players };
    }

    [HttpGet("player", Name = "GetPlayerBrandPayments")]
    [Description("Get Brand Payments for player deposit")]
    public async IAsyncEnumerable<BrandPaymentResponse> GetPlayerBrandPaymentsAsync(
        [FromServices] IQueryService<BrandPaymentsQuery, BrandPaymentInfo> brandPaymentsQueryService,
        [FromServices] IGetService<AllPaymentMethodsQuery, IDictionary<Guid, PaymentMethodInfo>> getAllPaymentMethodService,
        [FromServices] IGetService<GetPaymentQuery, PaymentInfo> getPaymentService,
        [FromQuery] GetPlayerBrandPaymentsRequest request)
    {
        var paymentMethods = await getAllPaymentMethodService
            .GetAsync(new AllPaymentMethodsQuery())
            .ConfigureAwait(false);

        var query = new BrandPaymentsQuery(request.BrandId, request.PlayerCode);

        var brandPayments = await brandPaymentsQueryService.QueryAsync(query)
            .ToArrayAsync()
            .ConfigureAwait(false);

        var transferType = request.TransferType == PaymentTransferType.Deposit
            ? TransferType.Deposit
            : TransferType.Withdraw;

        var brandPaymentQuery = await brandPayments
            .Where(bp => bp.TransferType == transferType)
            .AsParallel()
            .WithCancellation(HttpContext.RequestAborted)
            .Select(async bp =>
            {
                var getPaymentQuery = new GetPaymentQuery(bp.PaymentId);

                var paymentInfo = await getPaymentService
                    .GetAsync(getPaymentQuery)
                    .ConfigureAwait(false);

                var paymentMethod = paymentMethods[bp.MethodId];

                return new BrandPaymentResponse
                {
                    Id = bp.Id,
                    ChannelId = bp.ChannelId,
                    ChannelName = bp.ChannelName,
                    ChannelCode = bp.ChannelCode,
                    TransferLimit = new ValueRange<decimal>
                    {
                        From = bp.TransferLimit.From,
                        To = bp.TransferLimit.To
                    },
                    ExtendedData = paymentInfo.Code == "Manual"
                        ? JsonDocument.Parse(bp.ExtendedData)
                        : JsonDocument.Parse("{}"),
                    PaymentId = bp.PaymentId,
                    PaymentCode = paymentInfo.Code,
                    MethodId = bp.MethodId,
                    MethodCode = paymentMethod.Name,
                    BankType = paymentMethod.BankType,
                    DisplayOrder = bp.DisplayOrder
                };
            })
            .WhenAll()
            .ConfigureAwait(false);

        foreach (var bp in brandPaymentQuery)
        {
            yield return bp;
        }
    }

    [HttpPut("{brandPaymentId:guid}")]
    [Description("Update a Payment")]
    public async Task<CreateUpdateBrandPaymentResponse> UpdateBrandPaymentAsync(
        Guid brandPaymentId,
        UpdateBrandPaymentRequest request)
    {
        if (request.Whitelist != null && request.Blacklist != null)
        {
            var duplicatePlayers = request.Whitelist.Intersect(request.Blacklist).ToArray();
            if (duplicatePlayers.Length != 0)
            {
                return new()
                {
                    Valid = false,
                    Message = Resources.BrandPayment.WhiteSameAsBlack,
                    DuplicatePlayers = duplicatePlayers
                };
            }
        }

        var brandPayment = await brandPaymentService.GetPaymentAsync(q => q.Id == brandPaymentId, tracking: true);

        if (brandPayment == null)
        {
            return CreateInvalidResponse<CreateUpdateBrandPaymentResponse>(Resources.BrandPayment.NotFound);
        }

        await brandPaymentService.UpdatePaymentAsync(brandPayment, request, Username);

        return CreateValidResponse<CreateUpdateBrandPaymentResponse>();
    }

    [HttpPatch("extendeddata/{brandPaymentId:guid}")]
    [Description("Update a brand Payment's extended data")]
    public async ValueTask UpdateBrandPaymentExtendedDataAsync(
        Guid brandPaymentId,
        UpdateBrandPaymentExtendedDataRequest request,
        [FromServices] IPaymentServiceClient paymentServiceClient) 
        {
            await paymentServiceClient.UpdateBrandPaymentExtendedDataAsync(brandPaymentId, request.ExtendedData, HttpContext.RequestAborted);
            var adminLogMessage = $"User: {Username} | Action: Edit brand payment extended data";
            LogAdminBrandpaymentSettingAccess(brandPaymentId, adminLogMessage);
        }

    [HttpPost("upload")]
    [Description("Upload player file")]
    public async Task<MessageResponse> UploadPlayersAsync(IFormFile file)
    {
        var uploadedResult = await uploadService.GetUploadedExcelAsync<UploadPlayerModel>(file);
        var users = uploadedResult.Where(p => !string.IsNullOrEmpty(p.Username))
            .Select(p => p.Username);

        var allPlayers = await playerQueries.GetQueries()
            .Where(p => p.Status == PlayerStatus.Enabled && users.Contains(p.Username))
            .Select(p => p.Username)
            .ToArrayAsync();

        var diffUsers = users.Except(allPlayers).ToArray();
        var distinctUsers = users.Distinct().ToArray();

        return diffUsers.Length != 0
            ? new() { Valid = false, Message = Resources.BrandPayment.PlayersNotFound, NotExistsPlayers = diffUsers }
            : new UploadFileResponse { Valid = true, Players = distinctUsers };
    }

    [HttpPatch("autowithdraw/{brandId:guid}")]
    [Description("Add or update a brand auto withdraw setting")]
    public async ValueTask<MessageResponse> UpsertBrandAutoWithdrawAsync(
        Guid brandId,
        [FromServices] IPaymentServiceClient paymentServiceClient,
        [FromBody] UpsertBrandAutoWithdrawRequest request)
    {
        await paymentServiceClient.UpdateBrandAutoWithdrawAsync(
            brandId,
            request.EnableAutoWithdraw,
            request.AmountUpperLimit,
            request.BrandPaymentIds,
            UserId,
            HttpContext.RequestAborted).ConfigureAwait(false);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpGet("balance/{brandId:Guid}", Name = "GetPaymentBalances")]
    public async IAsyncEnumerable<BrandPaymentBalanceViewModel> GetPaymentBalancesAsync(
        [FromServices] IGetService<GetBrandPaymentBalanceQuery, decimal?> getBalanceService,
        Guid brandId,
        [FromQuery(Name = "id")] IEnumerable<Guid> brandPaymentIds)
    {
        foreach (var id in brandPaymentIds)
        {
            var query = new GetBrandPaymentBalanceQuery(brandId, id);

            var balance = await getBalanceService.GetAsync(
                query,
                HttpContext.RequestAborted).ConfigureAwait(false);

            yield return new BrandPaymentBalanceViewModel
            {
                BrandPaymentId = id,
                Balance = balance
            };
        }
    }

    private async Task<CreateUpdateBrandPaymentResponse> CreateBrandPaymentAsync(
        PaymentTransferType transferType,
        CreateBrandPaymentRequest request)
    {
        if (request.Whitelist != null && request.Blacklist != null)
        {
            var duplicatePlayers = request.Whitelist.Intersect(request.Blacklist).ToArray();
            if (duplicatePlayers.Length != 0)
            {
                return new()
                {
                    Valid = false,
                    Message = Resources.BrandPayment.WhiteSameAsBlack,
                    DuplicatePlayers = duplicatePlayers
                };
            }
        }

        await brandPaymentService.CreatePaymentAsync(request, transferType, Username);
        return CreateValidResponse<CreateUpdateBrandPaymentResponse>();
    }

    private void LogAdminBrandpaymentSettingAccess(Guid brandPaymentId, string message) {
        var ip = HttpContext.Connection.RemoteIpAddress;
        var adminLogEvent = new AdminLogEvent() {
            TrackingId = brandPaymentId,
            ServerName = Environment.MachineName,
            IpAddress = ip is null ? "" : ip.ToString(),
            Description = message,
            Method = HttpContext.Request.Method,
            Path = HttpContext.Request.Path,
            RecordedOn = DateTimeOffset.Now,
            ResponseCode = (short)HttpContext.Response.StatusCode
        };
        tgsLog.LogAdmin(adminLogEvent);
    }
}
