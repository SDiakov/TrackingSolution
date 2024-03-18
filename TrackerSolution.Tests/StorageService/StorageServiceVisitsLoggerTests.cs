using Common;
using System.Globalization;
using StorageService;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TrackerSolution.IntegrationTests.StorageService
{
    [TestClass]
    public class StorageServiceVisitsLoggerTests
    {
        private string logFilePath = "/testVisitLog.txt";
        private string formattedLogString = "{0}|{1}|{2}|{3}";

        [TestInitialize]
        public void InitializeTest()
        {
            File.Create(logFilePath).Close();
        }

        [TestCleanup]
        public void CleanupTest()
        {
            File.Delete(logFilePath);
        }

        [TestMethod]
        public async Task SendMessageTest_VisitsReceived_StringsLogged()
        {
            // Arrange
            var expectedMessage1 = new VisitMessage
            {
                DateTime = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                Ip = "127.0.0.1",
                Referrer = "Host1",
                UserAgent = "Edge"
            };
            var expectedMessage2 = new VisitMessage
            {
                DateTime = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                Ip = "127.0.0.2",
                Referrer = "Host2",
                UserAgent = "Chrome"
            };

            var logger = new VisitLogger(logFilePath);

            // Act
            logger.Process(expectedMessage1);
            logger.Process(expectedMessage2);

            // Assert
            var expectedString1 = string.Format(formattedLogString,
                expectedMessage1.DateTime, expectedMessage1.Referrer, expectedMessage1.UserAgent, expectedMessage1.Ip);
            var expectedString2 = string.Format(formattedLogString,
                expectedMessage2.DateTime, expectedMessage2.Referrer, expectedMessage2.UserAgent, expectedMessage2.Ip);

            var actualLogStrings = File.ReadAllLines(logFilePath);

            Assert.AreEqual(expectedString1, actualLogStrings[0]);
            Assert.AreEqual(expectedString2, actualLogStrings[1]);
        }

        [TestMethod]
        public async Task SendMessageTest_VisitsWithEmptyFieldsReceived_StringsWithNullLogged()
        {
            // Arrange
            var expectedMessage1 = new VisitMessage
            {
                DateTime = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                Ip = "127.0.0.1",
                Referrer = "",
                UserAgent = "Edge"
            };
            var expectedMessage2 = new VisitMessage
            {
                DateTime = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                Ip = "127.0.0.2",
                Referrer = "Host2",
                UserAgent = null
            };

            var logger = new VisitLogger(logFilePath);

            // Act
            logger.Process(expectedMessage1);
            logger.Process(expectedMessage2);

            // Assert
            var expectedString1 = string.Format(formattedLogString,
                expectedMessage1.DateTime, "null", expectedMessage1.UserAgent, expectedMessage1.Ip);
            var expectedString2 = string.Format(formattedLogString,
                expectedMessage2.DateTime, expectedMessage2.Referrer, "null", expectedMessage2.Ip);

            var actualLogStrings = File.ReadAllLines(logFilePath);

            Assert.AreEqual(expectedString1, actualLogStrings[0]);
            Assert.AreEqual(expectedString2, actualLogStrings[1]);
        }

        [TestMethod]
        public async Task SendMessageTest_VisitsWithoutIpReceived_NothingLogged()
        {
            // Arrange
            var expectedMessage1 = new VisitMessage
            {
                DateTime = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                Ip = null,
                Referrer = "Host1",
                UserAgent = "Edge"
            };
            var expectedMessage2 = new VisitMessage
            {
                DateTime = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                Ip = "",
                Referrer = "Host2",
                UserAgent = "Cgrome"
            };

            var logger = new VisitLogger(logFilePath);

            // Act
            logger.Process(expectedMessage1);
            logger.Process(expectedMessage2);

            // Assert

            var actualLogStrings = File.ReadAllLines(logFilePath);

            Assert.AreEqual(0, actualLogStrings.Length);
        }
    }
}