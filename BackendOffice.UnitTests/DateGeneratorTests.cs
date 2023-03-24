using FluentAssertions;
using TGS.Services.BackOffice;

namespace TGS.Tests.BackendOffice.UnitTests
{
    [TestFixture]
    public class DateGeneratorTests
    {
        [Test]
        public void GetFirstDayOfWeekTests()
        {
            var timeSpan = new TimeSpan(8, 0, 0);
            var date = new DateTimeOffset(2021, 6,
                3, 0,
                0, 0, timeSpan);

            var firstDay =
                DateGenerator.GetFirstDayOfWeek(date);

            firstDay.Should().Be(new DateTimeOffset(2021, 5,
                31, 0,
                0, 0, timeSpan));
        }

        [Test]
        public void GetLastDayOfWeekTests()
        {
            var timeSpan = new TimeSpan(8, 0, 0);
            var date = new DateTimeOffset(2021, 6,
                3, 0,
                0, 0, timeSpan);

            var firstDay =
                DateGenerator.GetLastDayOfWeek(date);

            firstDay.Should().Be(new DateTimeOffset(2021, 6,
                6, 0,
                0, 0, timeSpan));
        }

        [Test]
        public void GetFirstDayOfMonthTests()
        {
            var timeSpan = new TimeSpan(8, 0, 0);
            var date = new DateTimeOffset(2021, 6,
                3, 0,
                0, 0, timeSpan);

            var firstDay =
                DateGenerator.GetFirstDayOfMonth(date);

            firstDay.Should().Be(new DateTimeOffset(2021, 6,
                1, 0,
                0, 0, timeSpan));
        }

        [Test]
        public void GetLastDayOfMonthTests()
        {
            var timeSpan = new TimeSpan(8, 0, 0);
            var date = new DateTimeOffset(2021, 6,
                3, 0,
                0, 0, timeSpan);

            var firstDay =
                DateGenerator.GetLastDayOfMonth(date);

            firstDay.Should().Be(new DateTimeOffset(2021, 6,
                30, 0,
                0, 0, timeSpan));
        }

        [Test]
        public void GetSpecifyDayOfMonthTests()
        {
            var timeSpan = new TimeSpan(8, 0, 0);
            var date = new DateTimeOffset(2021, 6,
                3, 0,
                0, 0, timeSpan);

            var firstDay =
                DateGenerator.GetSpecifyDayOfMonth(date, 16);

            firstDay.Should().Be(new DateTimeOffset(2021, 6,
                16, 0,
                0, 0, timeSpan));
        }
    }
}