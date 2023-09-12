using System.Reflection;
using NSubstitute;
using TGS.Services.BackOffice.Application.Commands.Localization;
using TGS.Services.BackOffice.Application.Queries.Localization;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class SystemLocalizationTests
{
    [Test]
    public void Export_Localization_Strings_To_File()
    {
        var moqBackOfficeRepository = Substitute.For<IBackOfficeRepository>();

        var sut = new ExportLocalizationFileCommandService(moqBackOfficeRepository);

        var command = new ExportLocalizationFileCommand("test");

        _ = sut.GetAsync(command);

        _ = moqBackOfficeRepository.Received(1).ExportLocalizationStringsAsync(Arg.Any<string>());
    }

    [Test]
    public async Task Import_Localization_File_To_System()
    {
        var moqBackOfficeRepository = Substitute.For<IBackOfficeRepository>();

        var sut = new ImportLocalizationFileCommandService(moqBackOfficeRepository);

        var command = new ImportLocalizationFileCommand(
            "test",
            Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(
                    "TGS.Tests.BackendOffice.UnitTests.Assets.TestLocalizationStrings.xlsx"));

        await sut.ExecuteAsync(command);

        _ = moqBackOfficeRepository.Received(1).ImportLocalizationStringsAsync(
            Arg.Any<string>(),
            Arg.Any<IEnumerable<LocalizationStringInfo>>());
    }
}