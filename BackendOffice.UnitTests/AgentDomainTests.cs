using Moq;
using Tgs.DomainSettings.Clients;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.ApplicationServices.Agents;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests;

public class AgentDomainTests
{
    [Test]
    public async Task Add_Domain_To_A_Agent()
    {
        var moqAgentRepository = new Mock<IAgentRepository>();

        var sut = new AddAgentDomainCommandService(moqAgentRepository.Object);

        var command = new AddAgentDomainCommand(
            Guid.NewGuid(),
            "test.com");

        await sut.ExecuteAsync(command);

        moqAgentRepository.Verify(
            fake => fake.AddDomainToAgentAsync(
                It.Is<Guid>(x => x == command.AgentId),
                It.Is<string>(x => x == command.DomainName)),
            Times.Exactly(1));
    }

    [Test]
    public async Task Apply_Ingress_For_AgentDomain()
    {
        var moqDomainSettingClient = new Mock<IDomainSettingClient>();

        var sut = new ApplyAgentDomainIngressCommandService(moqDomainSettingClient.Object);

        var command = new ApplyAgentDomainIngressCommand(Guid.NewGuid());

        await sut.ExecuteAsync(command);

        moqDomainSettingClient.Verify(
            fake => fake.ApplyAgentDomainIngressAsync(It.Is<Guid>(x => x == command.AgentId)),
            Times.Exactly(1));
    }

    [Test]
    public async Task Create_Dns_Zone_For_AgentDomain()
    {
        var moqDomainSettingClient = new Mock<IDomainSettingClient>();

        var sut = new CreateAgentDnsZoneCommandService(moqDomainSettingClient.Object);

        var command = new CreateAgentDnsZoneCommand(Guid.NewGuid());

        await sut.ExecuteAsync(command);

        moqDomainSettingClient.Verify(
            fake => fake.CreateZoneAsync(It.Is<Guid>(x => x == command.AgentId)),
            Times.Exactly(1));
    }

    [Test]
    public async Task Delete_Dns_Zone_For_AgentDomain()
    {
        var moqDomainSettingClient = new Mock<IDomainSettingClient>();

        var sut = new DeleteAgentDnsZoneCommandService(moqDomainSettingClient.Object);

        var command = new DeleteAgentDnsZoneCommand(Guid.NewGuid());

        await sut.ExecuteAsync(command);

        moqDomainSettingClient.Verify(
            fake => fake.DeleteZoneAsync(It.Is<Guid>(x => x == command.AgentId)),
            Times.Exactly(1));
    }

    [Test]
    public async Task Delete_Ingress_For_AgentDomain()
    {
        var moqDomainSettingClient = new Mock<IDomainSettingClient>();

        var sut = new DeleteAgentDomainIngressCommandService(moqDomainSettingClient.Object);

        var command = new DeleteAgentDomainIngressCommand(Guid.NewGuid());

        await sut.ExecuteAsync(command);

        moqDomainSettingClient.Verify(
            fake => fake.DeleteAgentDomainIngressAsync(It.Is<Guid>(x => x == command.AgentId)),
            Times.Exactly(1));
    }

    [Test]
    public async Task Get_Domains_From_A_Agent()
    {
        var moqAgentRepository = new Mock<IAgentRepository>();

        var sut = new AgentDomainsQueryService(moqAgentRepository.Object);

        var query = new AgentDomainsQuery(Guid.NewGuid());

        _ = moqAgentRepository.Setup(fake => fake.QueryDomainsFromAgentAsync(It.IsAny<Guid>())).Returns(Array.Empty<string>().ToAsyncEnumerable());

        _ = await sut.QueryAsync(query).ToArrayAsync();

        moqAgentRepository.Verify(
            fake => fake.QueryDomainsFromAgentAsync(It.Is<Guid>(x => x == query.AgentId)),
            Times.Exactly(1));
    }

    [Test]
    public async Task Remove_Domain_From_A_Agent()
    {
        var moqAgentRepository = new Mock<IAgentRepository>();

        var sut = new RemoveAgentDomainCommandService(moqAgentRepository.Object);

        var command = new RemoveAgentDomainCommand(
            Guid.NewGuid(),
            "test.com");

        await sut.ExecuteAsync(command);

        moqAgentRepository.Verify(
            fake => fake.RemoveDomainFromAgentAsync(
                It.Is<Guid>(x => x == command.AgentId),
                It.Is<string>(x => x == command.DomainName)),
            Times.Exactly(1));
    }
}
