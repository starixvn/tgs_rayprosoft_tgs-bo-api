using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Services.BackOffice.Application.Models.PlatformCredit;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IPlatformCreditService
{
    Task<PlatformCredit> GetPlatformCreditAsync(
        Expression<Func<PlatformCredit, bool>> predicate,
        Func<IQueryable<PlatformCredit>,
            IQueryable<PlatformCredit>> preQuery = null);

    IQueryable<PlatformCredit> GetPlatformCreditQueries(
        Func<IQueryable<PlatformCredit>,
            IQueryable<PlatformCredit>> preQuery = null,
        bool tracking = false);

    IQueryable<PlatformPayRecord> GetPlatformCreditPaymentQueries(
        Func<IQueryable<PlatformPayRecord>,
            IQueryable<PlatformPayRecord>> preQuery = null,
        bool tracking = false);

    Task UpdatePlatformCreditAsync(PlatformCredit platformCredit,
        UpdatePlatformCreditRequest request);

    Task PayAsync(PlatformCredit platformCredit, PayRequest request);
}

public class PlatformCreditService : BaseRepositoryOperations<ITgsRepository>, IPlatformCreditService
{
    private readonly ITgsRepository _repository;

    public PlatformCreditService(
        ITgsRepository tgsRepository) : base(
        tgsRepository)
        => _repository = tgsRepository;

    IQueryable<PlatformCredit> IPlatformCreditService.GetPlatformCreditQueries(
        Func<IQueryable<PlatformCredit>,
            IQueryable<PlatformCredit>> preQuery,
        bool tracking) =>
        GetQueriesAsync(preQuery, tracking);

    Task<PlatformCredit> IPlatformCreditService.GetPlatformCreditAsync(
        Expression<Func<PlatformCredit, bool>> predicate,
        Func<IQueryable<PlatformCredit>,
            IQueryable<PlatformCredit>> preQuery) =>
        GetEntityFirstOrDefaultAsync(predicate, preQuery);

    async Task IPlatformCreditService.PayAsync(PlatformCredit platformCredit,
        PayRequest request)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        platformCredit.Balance += request.Amount;

        var payRecord = new PlatformPayRecord
        {
            PlatformCreditId = platformCredit.Id,
            PayAmount = request.Amount,
            Balance = platformCredit.Balance,
            CreatedByUsername = request.EditorUsername,
            CreatedOn = DateTimeOffset.Now
        };
        await UpdateEntityAsync(platformCredit);

        await CreateEntityAsync(payRecord);

        scope.Complete();
    }

    async Task IPlatformCreditService.UpdatePlatformCreditAsync(
        PlatformCredit platformCredit,
        UpdatePlatformCreditRequest request)
    {
        var balance = await GetBalanceAsync(platformCredit.BrandPayRecords, platformCredit.BrandId, request.Credit);

        platformCredit.Credit = request.Credit;
        platformCredit.Balance = balance;
        platformCredit.UpdatedByUsername = request.EditorUsername;
        platformCredit.UpdatedOn = DateTimeOffset.Now;

        await UpdateEntityAsync(platformCredit);
    }

    IQueryable<PlatformPayRecord> IPlatformCreditService.GetPlatformCreditPaymentQueries(
        Func<IQueryable<PlatformPayRecord>, IQueryable<PlatformPayRecord>> preQuery, bool tracking)
        => GetQueriesAsync(preQuery, tracking);

    private async ValueTask<decimal> GetBalanceAsync(IEnumerable<PlatformPayRecord> records, Guid brandId,
        decimal amount)
    {
        var previousDay = DateTimeOffset.Now.ToOffset(new(8, 0, 0)).AddDays(-1);

        var reportDate = new DateTimeOffset(previousDay.Year,
            previousDay.Month,
            previousDay.Day,
            0,
            0,
            0,
            previousDay.Offset);

        var firstDayOfMonth = new DateTimeOffset(reportDate.Year, reportDate.Month, 1, 0, 0, 0, new(8, 0, 0));

        var netProfit = await _repository.PlayerAggregates.AsQueryable()
            .Where(x => x.BrandId == brandId)
            .Where(x => x.ReportDate >= firstDayOfMonth && x.ReportDate <= reportDate)
            .SumAsync(x => x.WinLossAmount - x.CompanyAdjustAmount -
                x.RebateAmount -
                x.PromotionAmount);

        var isFirstDay = DateTimeOffset.Now.ToOffset(new(8, 0, 0)).Day == 1;

        var payTime = isFirstDay ? reportDate : DateTimeOffset.Now;

        var pay = records
            .Where(p => p.CreatedOn >= firstDayOfMonth && p.CreatedOn < payTime).Sum(p => p.PayAmount);

        return amount - netProfit + pay;
    }
}