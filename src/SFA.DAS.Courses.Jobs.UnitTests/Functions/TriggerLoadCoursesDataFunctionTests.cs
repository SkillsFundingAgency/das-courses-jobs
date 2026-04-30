using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Courses.Infrastructure.Api;
using SFA.DAS.Courses.Infrastructure.Configuration;
using SFA.DAS.Courses.Jobs.Functions;

namespace SFA.DAS.Courses.Jobs.UnitTests.Functions;

public class TriggerLoadCoursesDataFunctionTests
{
    [Test]
    public async Task Run_Should_Call_LoadCoursesData()
    {
        // Arrange
        var coursesApiMock = new Mock<ICoursesApi>();
        var configuration = Options.Create(new ApplicationConfiguration { FunctionsConfiguration = new FunctionsConfiguration { LoadCoursesTimeoutInMinutes = 5 } });
        var sut = new TriggerLoadCoursesDataFunction(coursesApiMock.Object, configuration);
        // Act
        await sut.Run(new TimerInfo());
        // Assert
        coursesApiMock.Verify(x => x.LoadCoursesData(It.IsAny<CancellationToken>()), Times.Once);
    }
}
