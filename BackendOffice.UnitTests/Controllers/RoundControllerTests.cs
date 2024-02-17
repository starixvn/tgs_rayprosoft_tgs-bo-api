using System.Data.SqlClient;
using NSubstitute;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Round;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Queries.Game;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class RoundControllerTests : BaseTests
{
    [SetUp]
    public override void Setup() => base.Setup();

    [Test]
    public async Task Can_Get_Rounds()
    {
        var moqReportClient = Substitute.For<IReportClient>();

        var sut = new GetGameRoundListQueryService(moqReportClient);

        var query = new GetGameRoundListQuery(
            Guid.NewGuid(),
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow,
            null,
            null,
            new()
            {
                Page = 1,
                Rows = 10,
                SortBy = "updatedOn",
                OrderBy = SortOrder.Descending,
                ShowAll = false
            });

        _ = await sut.GetAsync(query);

        _ = moqReportClient.Received(1).GetRoundsAsync(Arg.Any<GetRoundsRequest>());
    }
}