using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Tgs.Core.Infrastructures.Security;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Infrastructures.Redis;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Commands.PlayerIdentity;
using TGS.Services.BackOffice.Application.Models.Player;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IPlayerService
{
    ValueTask<Guid> CreatePlayerAuthenticationFormAsync(Guid brandId, string playerCode, string data);

    Task<bool> ExistsAsync(Expression<Func<Player, bool>> predicate);

    Task<Player> GetPlayerAsync(
        Expression<Func<Player, bool>> predicate,
        Func<IQueryable<Player>, IQueryable<Player>> preQuery = null);

    ValueTask<PlayerIdentityFormModel> GetPlayerIdentityFormAsync(string playerCode);

    ValueTask<PlayerIdentityFormInfo> GetPlayerIdentityFormByIdAsync(Guid formId);

    IQueryable<PlayerAuditLog> GetPlayerOperationQueries(
        Func<IQueryable<PlayerAuditLog>, IQueryable<PlayerAuditLog>> preQuery = null,
        bool tracking = false);

    IQueryable<Player> GetPlayerQueries(
        Func<IQueryable<Player>, IQueryable<Player>> preQuery = null,
        bool tracking = false);

    ValueTask SignOffPlayerIdentityFormAsync(
        Guid brandId,
        Guid formId,
        PlayerIdentityReviewState? reviewStatus,
        string reviewUserId,
        string reviewUserName,
        string remark,
        string realName,
        string idNumber);

    Task TopUpAccumulatedDepositAsync(string playerCode, decimal amount);

    Task UpdateParentPlayerAsync(string playerCode, string parentPlayerCode);

    Task UpdatePlayerAsync(Player player, UpdatePlayerRequest request);

    ValueTask UpdatePlayerIdentityAsync(string playerCode, Guid identityAuthenticationFormId);
}

public class PlayerService(
    ITgsRepository tgsRepository,
    IPasswordHandler passwordHandler,
    IRedisCacheManager cacheManager) : BaseRepositoryOperations<ITgsRepository>(
    tgsRepository), IPlayerService
{
    private readonly IRedisCacheManager _cacheManager = cacheManager;
    private readonly IPasswordHandler _passwordHandler = passwordHandler;
    private readonly ITgsRepository _repository = tgsRepository;

    public async ValueTask<Guid> CreatePlayerAuthenticationFormAsync(Guid brandId, string playerCode, string data)
    {
        var createFormId = Guid.NewGuid();
        var currentDate = DateTimeOffset.UtcNow;

        _ = await _repository.PlayerAuthenticationForms
            .AddAsync(new PlayerAuthenticationForm
            {
                Id = createFormId,
                CreatedOn = currentDate,
                BrandId = brandId,
                PlayerCode = playerCode,
                Data = data
            })
            .ConfigureAwait(false);

        _ = await _repository
            .SaveChangesAsync()
            .ConfigureAwait(false);

        return createFormId;
    }

    Task<bool> IPlayerService.ExistsAsync(Expression<Func<Player, bool>> predicate) =>
                ExistsAsync(predicate);

    Task<Player> IPlayerService.GetPlayerAsync(
        Expression<Func<Player, bool>> predicate,
        Func<IQueryable<Player>, IQueryable<Player>> preQuery) =>
        GetEntityFirstOrDefaultAsync(predicate, preQuery);

    async ValueTask<PlayerIdentityFormModel> IPlayerService.GetPlayerIdentityFormAsync(string playerCode)
    {
        var player = await _repository.Players
          .FindAsync(playerCode)
          .ConfigureAwait(false);

        var playerIdentity = await _repository.PlayerIdentity
            .FindAsync(playerCode)
            .ConfigureAwait(false);

        var form = await _repository.PlayerAuthenticationForms
            .FirstOrDefaultAsync(paf => paf.Id == playerIdentity.PlayerAuthenticationFormId)
            .ConfigureAwait(false);

        return new PlayerIdentityFormModel
        {
            Id = form.Id,
            RealName = player.RealName,
            IdNumber = player.IdNumber,
            Data = form.Data,
            CreatedOn = form.CreatedOn,
            Remark = form.Remark,
            ReviewedOn = form.ReviewedOn,
            ReviewedUserName = form.ReviewedUserName,
            Status = (PlayerIdentityReviewState)form.Status
        };
    }

    async ValueTask<PlayerIdentityFormInfo> IPlayerService.GetPlayerIdentityFormByIdAsync(Guid formId)
    {
        var form = await _repository.PlayerAuthenticationForms
            .SingleOrDefaultAsync(paf => paf.Id == formId)
            .ConfigureAwait(false);

        return new PlayerIdentityFormInfo
        {
            FormId = form.Id,
            Status = (PlayerIdentityReviewState)form.Status
        };
    }

    IQueryable<PlayerAuditLog> IPlayerService.GetPlayerOperationQueries(
        Func<IQueryable<PlayerAuditLog>, IQueryable<PlayerAuditLog>> preQuery, bool tracking)
            => GetQueriesAsync(preQuery, tracking);

    IQueryable<Player> IPlayerService.GetPlayerQueries(
        Func<IQueryable<Player>, IQueryable<Player>> preQuery, bool tracking)
            => GetQueriesAsync(preQuery, tracking);

    async ValueTask IPlayerService.SignOffPlayerIdentityFormAsync(
        Guid brandId,
        Guid formId,
        PlayerIdentityReviewState? status,
        string reviewUserId,
        string reviewUserName,
        string remark,
        string realName,
        string idNumber)
    {
        var brand = await _repository.Brands
            .SingleAsync(b => b.Id == brandId)
            .ConfigureAwait(false);

        if (!brand.EnablePlayerIdentity)
            return;

        var formData = await _repository.PlayerAuthenticationForms
            .FindAsync(new object[] { formId })
            .ConfigureAwait(false);

        var playerCode = formData.PlayerCode;

        if (formData == null)
            return;

        formData.Status = status.HasValue ? (int)status.Value : formData.Status;
        formData.ReviewedOn = DateTimeOffset.UtcNow;
        formData.Remark = remark;
        formData.ReviewedUserId = reviewUserId;
        formData.ReviewedUserName = reviewUserName;

        if (status == PlayerIdentityReviewState.Approve)
        {
            var player = await _repository.Players.SingleAsync(p => p.PlayerCode == playerCode);
            player.IdentityAuthentication = true;
            player.RealName = realName;
            player.IdNumber = idNumber;
            player.UpdatedByUsername = reviewUserName;
            player.UpdatedOn = DateTimeOffset.Now;
        }

        _ = await _repository.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task TopUpAccumulatedDepositAsync(string playerCode, decimal amount)
    {
        var player = await _repository.Players
            .FindAsync(playerCode)
            .ConfigureAwait(false);

        player.AccumulatedDeposit += amount;

        player.UpdatedByUsername = playerCode;
        player.UpdatedOn = DateTimeOffset.Now;

        var auditLog = new PlayerAuditLog
        {
            Id = Guid.NewGuid(),
            ModuleName = "Player",
            CreatedByUsername = playerCode,
            CreatedOn = DateTimeOffset.Now,
            Operation = "Update Player AccumulatedDeposit",
            Username = playerCode
        };

        _ = await _repository.PlayerAuditLogs
            .AddAsync(auditLog)
            .ConfigureAwait(false);

        _ = await _repository.SaveChangesAsync().ConfigureAwait(false);

        await _cacheManager
            .ClearCacheAsync(RedisCacheType.Player, playerCode)
            .ConfigureAwait(false);
    }

    async Task IPlayerService.UpdateParentPlayerAsync(string playerCode, string parentPlayerCode)
    {
        var player = await _repository.PlayerParents
            .FirstOrDefaultAsync(pp => pp.PlayerCode == playerCode)
            .ConfigureAwait(false);

        if (player != null)
        {
            player.ParentPlayerCode = parentPlayerCode;
        }
        else
        {
            var playerParent = new PlayerParent
            {
                PlayerCode = playerCode,
                ParentPlayerCode = parentPlayerCode
            };
            _ = await _repository.PlayerParents.AddAsync(playerParent);
        }

        _ = await _repository.SaveChangesAsync().ConfigureAwait(false);
    }

    async Task IPlayerService.UpdatePlayerAsync(
        Player player,
        UpdatePlayerRequest request)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var updatePlayerType = new[]
        {
            "info",
            "contact",
            "password"
        };

        if (request.Type == updatePlayerType[0])
        {
            player.RealName = request.RealName;
            player.VipId = request.VipId;
            player.Status = request.Status;
            player.Remark = request.Remark;
        }

        if (request.Type == updatePlayerType[1])
        {
            player.AreaCode = request.AreaCode;
            player.Phone = request.Phone;
            player.Email = request.Email;
            player.QQ = request.QQ;
            player.Wechat = request.Wechat;
            player.PhoneAuthentication = player.Phone.IsNotNullOrEmpty();
        }

        if (request.Type == updatePlayerType[2])
        {
            var (salt, pwd) = _passwordHandler.GenerateCredential(request.Password);
            player.Password = pwd;
            player.Salt = salt;

            await _cacheManager
                .ClearCacheAsync(RedisCacheType.Player, request.PlayerCode)
                .ConfigureAwait(false);
        }

        player.UpdatedByUsername = request.EditorUsername;
        player.UpdatedOn = DateTimeOffset.Now;

        var auditLog = new PlayerAuditLog
        {
            Id = Guid.NewGuid(),
            ModuleName = "Player",
            CreatedByUsername = request.EditorUsername,
            CreatedOn = DateTimeOffset.Now,
            Operation = $"Update Player {request.Type}",
            Username = request.PlayerCode
        };

        await _cacheManager
            .ClearCacheAsync(RedisCacheType.Player, request.PlayerCode)
            .ConfigureAwait(false);

        await UpdateEntityAsync(player).ConfigureAwait(false);
        await CreateEntityAsync(auditLog).ConfigureAwait(false);

        scope.Complete();
    }

    public async ValueTask UpdatePlayerIdentityAsync(string playerCode, Guid identityAuthenticationFormId)
    {
        var identityData = await _repository.PlayerIdentity
        .FindAsync(playerCode)
        .ConfigureAwait(false);

        if (identityData == null)
        {
            identityData = new PlayerIdentity
            {
                PlayerCode = playerCode,
                PlayerAuthenticationFormId = identityAuthenticationFormId
            };

            _ = await _repository.PlayerIdentity
                .AddAsync(identityData)
                .ConfigureAwait(false);
        }
        else
        {
            identityData.PlayerAuthenticationFormId = identityAuthenticationFormId;
        }

        _ = await _repository
            .SaveChangesAsync()
            .ConfigureAwait(false);
    }
}
