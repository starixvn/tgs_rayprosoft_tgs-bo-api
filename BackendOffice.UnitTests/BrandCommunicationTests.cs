using System.Linq.Expressions;
using NSubstitute;
using NUnit.Framework.Legacy;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Commands.BrandCommunication;
using TGS.Services.BackOffice.Application.Queries.BrandCommunication;
using CoreModels = TGS.Core.DataServices.Tgs.Models;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class BrandCommunicationTests
{
    [Test]
    public async Task Create_Brand_Communication_Should_Success()
    {
        var fakeTgsCommand = Substitute.For<ITgsCommands<CoreModels.BrandCommunication>>();

        var sut = new CreateBrandCommunicationCommandService(fakeTgsCommand);

        var command = new CreateBrandCommunicationCommand();
        await sut.ExecuteAsync(command);

        _ = fakeTgsCommand
            .Received(1)
            .CreateAsync(Arg.Any<CoreModels.BrandCommunication>());
    }

    [Test]
    public async Task Delete_Brand_Communication_Should_Success()
    {
        var fakeTgsQueries = Substitute.For<ITgsQueries<CoreModels.BrandCommunication>>();
        var fakeTgsCommand = Substitute.For<ITgsCommands<CoreModels.BrandCommunication>>();

        var sut = new DeleteBrandCommunicationCommandService(fakeTgsQueries, fakeTgsCommand);

        var command = new DeleteBrandCommunicationCommand(Guid.NewGuid());
        await sut.ExecuteAsync(command);

        _ = fakeTgsCommand
            .Received(1)
            .DeleteAsync(Arg.Any<CoreModels.BrandCommunication>());
    }

    [Test]
    public async Task Get_Brand_Communications_Should_Success()
    {
        var fakeTgsQueries = Substitute.For<ITgsQueries<CoreModels.BrandCommunication>>();
        var sut = new GetBrandCommunicationQueryService(fakeTgsQueries);

        var query = new BrandCommunicationQuery(
            BrandId: Guid.NewGuid());
        _ = await sut.QueryAsync(query)
            .ToListAsync()
            .ConfigureAwait(false);

        _ = fakeTgsQueries
            .Received(1)
            .GetQueries();
    }

    [Test]
    public void String_To_CommunicationType_If_Not_Match_Parse_Fail()
    {
        var text = "AAAA";
        var actual = Enum.TryParse(typeof(CommunicationType), text, out _);

        ClassicAssert.IsFalse(actual);
    }

    [Test]
    public async Task Update_Brand_Communication_Should_Success()
    {
        var fakeTgsQueries = Substitute.For<ITgsQueries<CoreModels.BrandCommunication>>();
        var fakeTgsCommand = Substitute.For<ITgsCommands<CoreModels.BrandCommunication>>();

        _ = fakeTgsQueries
            .GetSingleAsync(Arg.Any<Expression<Func<CoreModels.BrandCommunication, bool>>>())
            .Returns(new CoreModels.BrandCommunication());

        var sut = new UpdateBrandCommunicationCommandService(fakeTgsQueries, fakeTgsCommand);

        var command = new UpdateBrandCommunicationCommand()
        {
            Id = Guid.NewGuid(),
            CommunicationType = CommunicationType.WeChat.ToString(),
            AppdId = "AppId",
            Phone = "Phone",
            QRCodeUrl = "https://www.google.com"
        };

        await sut.ExecuteAsync(command);

        _ = fakeTgsCommand
            .Received(1)
            .UpdateAsync(Arg.Any<CoreModels.BrandCommunication>());
    }
}
