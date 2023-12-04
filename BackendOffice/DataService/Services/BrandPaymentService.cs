using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TGS.Apis.BackendOffice.Controllers.Messages;
using TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.DataService.Services;

public class BrandPaymentService(ITgsRepository repository) : IBrandPaymentService
{
    public async Task ChangeOrderAsync(ChangeOrderRequest request)
    {
        var ids = request.Orders.Select(q => q.Id);
        var payments = await repository.BrandPayments.AsQueryable()
            .Where(q => ids.Contains(q.Id)).ToListAsync();

        payments.ForEach(p =>
        {
            var item = request.Orders.FirstOrDefault(x => x.Id == p.Id);

            if (item != null)
            {
                p.DisplayOrder = item.DisplayOrder;
            }
        });

        repository.BrandPayments.UpdateRange(payments);
        _ = await repository.SaveChangesAsync();
    }

    public async Task ChangeStatusAsync(BrandPayment payment, SwitchStatus status, string username)
    {
        var beforePayment = new BrandPayment
        {
            ChannelName = payment.ChannelName,
            PaymentStatus = payment.PaymentStatus,
            TransferLimitFrom = payment.TransferLimitFrom,
            TransferLimitTo = payment.TransferLimitTo,
            PlayerListCondition = payment.PlayerListCondition,
            AllPlayers = payment.AllPlayers,
            IdentityAuthentication = payment.IdentityAuthentication,
            PhoneAuthentication = payment.PhoneAuthentication,
            DepositedAmountUnlimited = payment.DepositedAmountUnlimited,
            DepositedAmountMin = payment.DepositedAmountMin,
            DepositedAmountMax = payment.DepositedAmountMax
        };

        payment.PaymentStatus = status;
        payment.UpdatedOn = DateTimeOffset.Now;
        payment.UpdatedByUsername = username;

        repository.Entry(payment).State = EntityState.Modified;

        _ = await repository.PaymentAuditLogs
            .AddAsync(GeneratePaymentAuditLog(payment, beforePayment))
            .ConfigureAwait(false);

        _ = await repository.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task CreatePaymentAsync(CreateBrandPaymentRequest request, PaymentTransferType transferType, string username)
    {
        var paymentId = Guid.NewGuid();
        var payment = new BrandPayment
        {
            Id = paymentId,
            BrandId = request.BrandId,
            DisplayOrder = 99,
            PaymentMethodChannelId = request.PaymentMethodChannelId,
            ChannelName = request.ChannelName,
            TransferType = transferType,
            ExtendedData = JsonSerializer.Serialize(request.ExtendedData),
            TransferLimitFrom = request.TransferLimitFrom,
            TransferLimitTo = request.TransferLimitTo,
            PlayerListCondition = request.PlayerListCondition,
            AllPlayers = request.AllPlayers,
            IdentityAuthentication = request.IdentityAuthentication,
            PhoneAuthentication = request.PhoneAuthentication,
            DepositedAmountUnlimited = request.DepositedAmountUnlimited,
            DepositedAmountMin = request.DepositedAmountMin,
            DepositedAmountMax = request.DepositedAmountMax,
            Reason = request.Reason,
            CreatedByUsername = username,
            UpdatedByUsername = username,
            CreatedOn = DateTimeOffset.Now,
            UpdatedOn = DateTimeOffset.Now,
            HandlingFeeRateTenPercentile = request.HandlingFeeRatePercentile * 10M,
            ReducePlayerAmount = request.ReducePlayerAmount,
        };

        var whitelist = await GeneratePaymentPlayerListAsync(
            paymentId,
            request.Whitelist,
            PlayerListType.White).ConfigureAwait(false);
        var blacklist = await GeneratePaymentPlayerListAsync(
            paymentId,
            request.Blacklist,
            PlayerListType.Black).ConfigureAwait(false);

        var playerList = whitelist.Union(blacklist);
        payment.BrandPaymentPlayers = new HashSet<BrandPaymentPlayer>(playerList);
        var vips = await repository.VipLevels.AsQueryable()
            .Where(x => request.VipLevelIds.Contains(x.Id))
            .ToArrayAsync();
        var vipLevels = vips.Select(vip => new BrandPaymentVipLevel
        {
            BrandPaymentId = paymentId,
            VipLevelId = vip.Id,
            VipLevel = vip
        }).ToArray();
        payment.VipLevels = new HashSet<BrandPaymentVipLevel>(vipLevels);
        _ = await repository.BrandPayments.AddAsync(payment);
        _ = await repository.SaveChangesAsync();
    }

    public Task<bool> ExistsAsync(Expression<Func<BrandPayment, bool>> predicate) =>
        repository.BrandPayments.AnyAsync(predicate);

    public Task<BrandPayment> GetPaymentAsync(Expression<Func<BrandPayment, bool>> predicate,
        Func<IQueryable<BrandPayment>, IQueryable<BrandPayment>> preQuery, bool tracking)
    {
        var payment = tracking
            ? repository.BrandPayments
            : repository.BrandPayments.AsNoTracking();
        preQuery ??= c => c;

        return preQuery(payment).FirstOrDefaultAsync(predicate);
    }

    public IQueryable<BrandPayment> GetPaymentQueries(bool tracking) =>
        tracking
            ? repository.BrandPayments
            : repository.BrandPayments.AsNoTracking();

    public async Task UpdatePaymentAsync(BrandPayment payment, UpdateBrandPaymentRequest request,
                                string username)
    {
        var beforePayment = new BrandPayment
        {
            ChannelName = payment.ChannelName,
            PaymentStatus = payment.PaymentStatus,
            TransferLimitFrom = payment.TransferLimitFrom,
            TransferLimitTo = payment.TransferLimitTo,
            PlayerListCondition = payment.PlayerListCondition,
            AllPlayers = payment.AllPlayers,
            IdentityAuthentication = payment.IdentityAuthentication,
            PhoneAuthentication = payment.PhoneAuthentication,
            DepositedAmountUnlimited = payment.DepositedAmountUnlimited,
            DepositedAmountMin = payment.DepositedAmountMin,
            DepositedAmountMax = payment.DepositedAmountMax,
            HandlingFeeRateTenPercentile = payment.HandlingFeeRateTenPercentile,
            ReducePlayerAmount = payment.ReducePlayerAmount,
        };

        payment.PaymentMethodChannelId = request.PaymentMethodChannelId;
        payment.ChannelName = request.ChannelName;
        payment.ExtendedData = JsonSerializer.Serialize(request.ExtendedData);
        payment.PaymentStatus = request.PaymentStatus;
        payment.TransferLimitFrom = request.TransferLimitFrom;
        payment.TransferLimitTo = request.TransferLimitTo;
        payment.PlayerListCondition = request.PlayerListCondition;
        payment.AllPlayers = request.AllPlayers;
        payment.IdentityAuthentication = request.IdentityAuthentication;
        payment.PhoneAuthentication = request.PhoneAuthentication;
        payment.DepositedAmountUnlimited = request.DepositedAmountUnlimited;
        payment.DepositedAmountMin = request.DepositedAmountMin;
        payment.DepositedAmountMax = request.DepositedAmountMax;
        payment.Reason = request.Reason;
        payment.UpdatedByUsername = username;
        payment.UpdatedOn = DateTimeOffset.Now;
        payment.HandlingFeeRateTenPercentile = request.HandlingFeeRatePercentile * 10M;
        payment.ReducePlayerAmount = request.ReducePlayerAmount;

        await using var tran = await repository.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        repository.BrandPaymentPlayers.RemoveRange(payment.BrandPaymentPlayers);
        repository.BrandPaymentVipLevels.RemoveRange(payment.VipLevels);

        _ = await repository.SaveChangesAsync();

        var whitelist = await GeneratePaymentPlayerListAsync(payment.Id, request.Whitelist, PlayerListType.White);
        var blacklist = await GeneratePaymentPlayerListAsync(payment.Id, request.Blacklist, PlayerListType.Black);
        var playerList = whitelist.Union(blacklist).ToArray();

        payment.BrandPaymentPlayers = new HashSet<BrandPaymentPlayer>(playerList);

        var vips = await repository.VipLevels.AsQueryable()
            .Where(x => request.VipLevelIds.Contains(x.Id)).ToArrayAsync();
        var vipLevels = vips.Select(vip => new BrandPaymentVipLevel
        {
            BrandPaymentId = payment.Id,
            VipLevelId = vip.Id,
            VipLevel = vip
        }).ToArray();

        payment.VipLevels = new HashSet<BrandPaymentVipLevel>(vipLevels);

        repository.Entry(payment).State = EntityState.Modified;

        _ = await repository.SaveChangesAsync();

        _ = await repository.PaymentAuditLogs.AddAsync(GeneratePaymentAuditLog(payment, beforePayment));
        _ = await repository.SaveChangesAsync();

        await tran.CommitAsync();
    }

    private static BrandPaymentAuditData ExchangeBrandPaymentData(BrandPayment payment) =>
        new(
            payment.ChannelName,
            payment.PaymentStatus,
            payment.TransferLimitFrom,
            payment.TransferLimitTo,
            payment.PlayerListCondition,
            payment.AllPlayers,
            payment.IdentityAuthentication,
            payment.PhoneAuthentication,
            payment.DepositedAmountMin,
            payment.DepositedAmountMax,
            payment.DepositedAmountUnlimited);

    private static BrandPaymentAuditLog GeneratePaymentAuditLog(BrandPayment payment, BrandPayment beforePayment) =>
        new()
        {
            Id = Guid.NewGuid(),
            BrandPaymentId = payment.Id,
            Before = JsonSerializer.Serialize(ExchangeBrandPaymentData(beforePayment)),
            After = JsonSerializer.Serialize(ExchangeBrandPaymentData(payment)),
            Reason = payment.Reason,
            CreatedOn = payment.UpdatedOn,
            CreatedByUsername = payment.UpdatedByUsername,
            Operation = BrandPaymentOperation.Update
        };

    private async Task<BrandPaymentPlayer[]> GeneratePaymentPlayerListAsync(Guid paymentId, string[] list,
                        PlayerListType playerListType)
    {
        var playerWhitelist = list != null
            ? await repository.Players.AsQueryable()
                .Where(x => list.Contains(x.Username)).ToArrayAsync()
            : [];
        return playerWhitelist.Select(player => new BrandPaymentPlayer
        {
            ListType = playerListType,
            BrandPaymentId = paymentId,
            PlayerCode = player.PlayerCode,
            Player = player
        }).ToArray();
    }
}
