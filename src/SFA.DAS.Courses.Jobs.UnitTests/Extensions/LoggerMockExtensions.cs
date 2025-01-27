using Microsoft.Extensions.Logging;
using Moq;
using System;

namespace SFA.DAS.Courses.Jobs.UnitTests.Extensions
{
    public static class LoggerMockExtensions
    {
        public static void VerifyLogging(this Mock<ILogger> loggerMock, LogLevel logLevel, string expectedMessage, Func<Times> times)
        {
            loggerMock.Verify(l => l.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedMessage)),
                It.IsAny<Exception>(),
                (Func<object, Exception, string>)It.IsAny<object>()),
                times);
        }

        public static void VerifyLogging<T>(this Mock<ILogger<T>> loggerMock, LogLevel logLevel, string expectedMessage, Func<Times> times)
        {
            loggerMock.Verify(l => l.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedMessage)),
                It.IsAny<Exception>(),
                (Func<object, Exception, string>)It.IsAny<object>()),
                times);
        }
    }
}