using System.Data;
using System.Threading.Tasks;
using Tgs.FileService.Clients;
using TGS.Apis.BackendOffice.Workers;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Models;

namespace TGS.Apis.BackendOffice.DataService.Services;

public interface IStationLetterService
{
    Task CreateStationLetterAsync(StationLetter stationLetter,
        StationLetterPlayer[] stationLetterPlayers);
}

public class StationLetterService : IStationLetterService
{
    private readonly IFileServiceClient _fileServiceClient;
    private readonly IStationLetterTaskQueue _letterTaskQueue;
    private readonly ITgsRepository _repository;

    public StationLetterService(ITgsRepository repository,
        IFileServiceClient fileServiceClient,
        IStationLetterTaskQueue letterTaskQueue)
    {
        _repository = repository;
        _fileServiceClient = fileServiceClient;
        _letterTaskQueue = letterTaskQueue;
    }

    async Task IStationLetterService.CreateStationLetterAsync(StationLetter stationLetter,
        StationLetterPlayer[] stationLetterPlayers)
    {
        var imageNames = stationLetter.Content.GetMatchStrings(Consts.UploadFile.ImagePattern);

        await using var tran = await _repository.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        stationLetter.Players = stationLetterPlayers;
        _ = await _repository.StationLetters.AddAsync(stationLetter);

        _ = await _repository.SaveChangesAsync();

        if (imageNames.Length > 0)
        {
            await _fileServiceClient.UpdateStationLetterFileAsync(Consts.UploadFile.StationLetter,
                stationLetter.Id.ToString(),
                new(imageNames));
        }

        if (stationLetter.AssignedAllPlayers)
        {
            await _letterTaskQueue.QueueStationLetterAsync(new StationLetterQueueModel(stationLetter.BrandId, stationLetter.Id));
        }

        await tran.CommitAsync();
    }
}
