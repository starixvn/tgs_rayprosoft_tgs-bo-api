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

public class StationLetterService(ITgsRepository repository,
    IFileServiceClient fileServiceClient,
    IStationLetterTaskQueue letterTaskQueue) : IStationLetterService
{
    async Task IStationLetterService.CreateStationLetterAsync(StationLetter stationLetter,
        StationLetterPlayer[] stationLetterPlayers)
    {
        var imageNames = stationLetter.Content.GetMatchStrings(Consts.UploadFile.ImagePattern);

        await using var tran = await repository.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        stationLetter.Players = stationLetterPlayers;
        _ = await repository.StationLetters.AddAsync(stationLetter);

        _ = await repository.SaveChangesAsync();

        if (imageNames.Length > 0)
        {
            await fileServiceClient.UpdateStationLetterFileAsync(Consts.UploadFile.StationLetter,
                stationLetter.Id.ToString(),
                new(imageNames));
        }

        if (stationLetter.AssignedAllPlayers)
        {
            await letterTaskQueue.QueueStationLetterAsync(new StationLetterQueueModel(stationLetter.BrandId, stationLetter.Id));
        }

        await tran.CommitAsync();
    }
}
