using TGS.Services.BackOffice.ApplicationServices.Promotion;

namespace TGS.Tests.BackendOffice.UnitTests;

internal class OfflineLoginRewardTests
{
    [Test]
    public async Task 下載Excel範本()
    {
        var sut = new ExportOfflineLoginRewardFormTemplateCommandService();

        var command = new ExportOfflineLoginRewardFormTemplateCommand();

        var actual = await sut.GetAsync(command);

        Assert.IsNotNull(actual);
    }
}