using System;
using System.Collections.Generic;
using System.Text;
using workshopdiomedes.Functions.Functions;
using workshopdiomedes.Tests.Helpers;
using Xunit;

namespace workshopdiomedes.Tests.Tests
{
    public class ScheduledFunctionTest
    {
        [Fact]
        public async void ScheduledFunction_Should_Log_Message()
        {
            // Arrange
            MockCloudTableWorkshop mockWorkshop = new MockCloudTableWorkshop(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            MockCloudTableConsolidated mockConsolidated = new MockCloudTableConsolidated(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));

            ListLogger logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);

            // Act
            await ScheduledFunction.Run(null,mockWorkshop,mockConsolidated,logger);
            string message = logger.Logs[0];

            // Asert
            Assert.Contains("Consolidated", message);
        }
    }
}
