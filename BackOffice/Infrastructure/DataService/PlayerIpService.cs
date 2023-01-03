using System;
using System.Linq;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.PlayerIp;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IPlayerIpService
{
    IQueryable<PlayerIpByIpModel> GetPlayerIpOperationByIpQueries(Guid brandId, string ipAddress, DateTimeOffset startOn, DateTimeOffset endOn);

    IQueryable<PlayerIpByUserNameModel> GetPlayerIpOperationByUsernameQueries(Guid brandId, string username, DateTimeOffset startOn, DateTimeOffset endOn);

    IQueryable<PlayerIpModel> GetPlayerIpOperationQueries(Guid brandId, DateTimeOffset startOn, DateTimeOffset endOn);

    IQueryable<PlayerIpRecord> GetPlayerIpRecordsByLogin(Guid brandId, string username);

    IQueryable<PlayerIpRecord> GetPlayerIpRecordsByLogin(string[] playerCodes);

    IQueryable<PlayerIpRecord> GetPlayerIpRecordsByRegister(Guid brandId, string username);

    IQueryable<PlayerIpModel> GetPlayerIpRecordsByRegisterQueries(Guid brandId, DateTimeOffset startOn, DateTimeOffset endOn);

    IQueryable<PlayerIpByIpModel> GetPlayerIpRecordsByRegisterQueries(Guid brandId, string ipAddress, DateTimeOffset startOn, DateTimeOffset endOn);
}

public class PlayerIpService : IPlayerIpService
{
    private readonly ITgsRepository _tgsRepository;

    public PlayerIpService(
        ITgsRepository tgsRepository)
    {
        _tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
    }

    public IQueryable<PlayerIpByIpModel> GetPlayerIpOperationByIpQueries(Guid brandId, string ipAddress, DateTimeOffset startOn, DateTimeOffset endOn)
        => _tgsRepository.PlayerLoginRecords.AsQueryable()
        .Where(x => x.BrandId == brandId &&
        x.IpAddress == ipAddress &&
        x.CreatedOn > startOn &&
        x.CreatedOn < endOn)
        .GroupBy(x => new
        {
            x.Player.PlayerCode,
            x.Player.Username
        })
        .Select(group => new PlayerIpByIpModel
        {
            PlayerCode = group.Key.PlayerCode,
            Username = group.Key.Username,
            CreatedOn = group.Max(y => y.CreatedOn)
        })
        .OrderBy(x => x.CreatedOn);

    public IQueryable<PlayerIpByUserNameModel> GetPlayerIpOperationByUsernameQueries(Guid brandId, string username, DateTimeOffset startOn, DateTimeOffset endOn)
        => _tgsRepository.PlayerLoginRecords.AsQueryable()
        .Where(x => x.BrandId == brandId &&
        x.Player.Username == username &&
        x.CreatedOn > startOn &&
        x.CreatedOn < endOn)
        .GroupBy(x => x.IpAddress)
        .Select(group => new PlayerIpByUserNameModel
        {
            IpAddress = group.Key,
            CreatedOn = group.Max(playerLoginRecord => playerLoginRecord.CreatedOn)
        })
        .OrderByDescending(x => x.CreatedOn);

    public IQueryable<PlayerIpModel> GetPlayerIpOperationQueries(Guid brandId, DateTimeOffset startOn, DateTimeOffset endOn)
                    => _tgsRepository.PlayerLoginRecords.AsQueryable()
           .Where(x =>
           x.BrandId == brandId &&
           x.CreatedOn > startOn &&
           x.CreatedOn < endOn)
           .GroupBy(x =>
           new
           {
               x.IpAddress,
               x.PlayerCode
           })
           .Select(group =>
           new
           {
               group.Key.IpAddress,
               group.Key.PlayerCode
           })
        .GroupBy(x => x.IpAddress)
        .Select(group => new PlayerIpModel
        {
            IpAddress = group.Key,
            MemberCount = group.Count()
        }).OrderByDescending(x => x.MemberCount);

    public IQueryable<PlayerIpRecord> GetPlayerIpRecordsByLogin(Guid brandId, string username)
        => from player in _tgsRepository.Players.AsQueryable()
           join playerIpRecord in _tgsRepository.PlayerIpRecords.AsQueryable()
           on player.PlayerCode equals playerIpRecord.PlayerCode
           where player.BrandId == brandId
           && playerIpRecord.IpType == (int)IpType.LoginIp
           && player.Username == username
           select playerIpRecord;

    public IQueryable<PlayerIpRecord> GetPlayerIpRecordsByLogin(string[] playerCodes)
        => _tgsRepository.PlayerIpRecords.AsQueryable()
        .Where(x => x.IpType == (int)IpType.LoginIp)
        .Where(x => playerCodes.Contains(x.PlayerCode));

    public IQueryable<PlayerIpRecord> GetPlayerIpRecordsByRegister(Guid brandId, string username)
                        => from player in _tgsRepository.Players.AsQueryable()
                           join playerIpRecord in _tgsRepository.PlayerIpRecords.AsQueryable()
                           on player.PlayerCode equals playerIpRecord.PlayerCode
                           where player.BrandId == brandId
                           && playerIpRecord.IpType == (int)IpType.RegisterIp
                           && player.Username == username
                           select playerIpRecord;

    public IQueryable<PlayerIpByIpModel> GetPlayerIpRecordsByRegisterQueries(Guid brandId, string ipAddress, DateTimeOffset startOn, DateTimeOffset endOn)
        => _tgsRepository.Players.AsQueryable()
        .Where(x => x.BrandId == brandId)
        .Join(_tgsRepository.PlayerIpRecords,
            player => player.PlayerCode,
            playerIpRecord => playerIpRecord.PlayerCode,
            (player, playerIpRecord) => new
            {
                player.BrandId,
                player.PlayerCode,
                player.Username,
                playerIpRecord.Ip,
                playerIpRecord.IpType,
                playerIpRecord.RecordOn
            })
        .Where(x =>
       x.IpType == (int)IpType.RegisterIp
       && x.Ip == ipAddress
       && x.RecordOn > startOn
       && x.RecordOn < endOn)
        .GroupBy(x => new
        {
            x.BrandId,
            x.PlayerCode,
            x.Username,
            x.Ip,
            x.IpType,
        })
        .Select((group) => new PlayerIpByIpModel
        {
            PlayerCode = group.Key.PlayerCode,
            Username = group.Key.Username,
            CreatedOn = group.Max(playerIpRecord => playerIpRecord.RecordOn)
        })
        .OrderBy(x => x.CreatedOn);

    public IQueryable<PlayerIpModel> GetPlayerIpRecordsByRegisterQueries(Guid brandId, DateTimeOffset startOn, DateTimeOffset endOn)
        => _tgsRepository.Players.AsQueryable()
        .Where(x => x.BrandId == brandId)
        .Join(_tgsRepository.PlayerIpRecords,
            player => player.PlayerCode,
            playerIpRecord => playerIpRecord.PlayerCode,
            (player, playerIpRecord) => new
            {
                player.BrandId,
                player.PlayerCode,
                player.Username,
                playerIpRecord.Ip,
                playerIpRecord.IpType,
                playerIpRecord.RecordOn
            })
        .Where(x =>
       x.IpType == (int)IpType.RegisterIp
       && x.RecordOn > startOn
       && x.RecordOn < endOn)
        .GroupBy(x => new
        {
            x.Ip,
            x.IpType,
            x.PlayerCode
        })
        .Select((group) => new
        {
            group.Key.Ip,
            group.Key.IpType,
            group.Key.PlayerCode,
        })
        .GroupBy(x => x.Ip)
        .Select(group => new PlayerIpModel
        {
            IpAddress = group.Key,
            MemberCount = group.Count()
        }).OrderByDescending(x => x.MemberCount);
}
