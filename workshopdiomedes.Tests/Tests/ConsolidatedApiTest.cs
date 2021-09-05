using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using workshopdiomedes.Functions.Entities;
using workshopdiomedes.Functions.Functions;
using workshopdiomedes.Tests.Helpers;
using Xunit;

namespace workshopdiomedes.Tests.Tests
{
    public class ConsolidatedApiTest
    {

        private readonly ILogger logger = TestFactory.CreateLogger();

        [Fact]
        public async void GetWorkshopById_Should_Return_200()
        {
            // Arrenge
            MockCloudTableConsolidated mockConsolidated = new MockCloudTableConsolidated(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            DateTime date = DateTime.UtcNow;
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(date);
            ConsolidatedEntity consolidatedEntity = TestFactory.GetConsolidatedEntity();

            // Act
            IActionResult response = await ConsolidatedApi.GetConsolidateByDate(request,mockConsolidated,date,logger);

            // Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }
    }
}
