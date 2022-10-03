using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using TGS.Services.BackOffice.Application.Commands.Localization;
using TGS.Services.BackOffice.Application.Queries.Localization;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class SystemLocalizationTests
{
    [Test]
    public async Task Import_Localization_File_To_System()
    {
        var moqBackOfficeRepository = new Mock<IBackOfficeRepository>();

        var sut = new ImportLocalizationFileCommandService(moqBackOfficeRepository.Object);

        var command = new ImportLocalizationFileCommand(
            "test",
            Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(
                "TGS.Tests.BackendOffice.UnitTests.Assets.TestLocalizationStrings.xlsx"));

        await sut.ExecuteAsync(command);

        moqBackOfficeRepository.Verify(
            fake => fake.ImportLocalizationStringsAsync(
                It.IsAny<string>(),
                It.IsAny<IEnumerable<LocalizationStringInfo>>()),
            Times.Exactly(1));
    }

    [Test]
    public void Export_Localization_Strings_To_File()
    {
        var moqBackOfficeRepository = new Mock<IBackOfficeRepository>();

        var sut = new ExportLocalizationFileCommandService(moqBackOfficeRepository.Object);

        var command = new ExportLocalizationFileCommand("test");

        _ = sut.GetAsync(command);

        moqBackOfficeRepository.Verify(
            fake => fake.ExportLocalizationStringsAsync(It.IsAny<string>()),
            Times.Exactly(1));
    }
}
