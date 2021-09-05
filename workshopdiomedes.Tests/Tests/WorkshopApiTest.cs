using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using workshopdiomedes.Common.Models;
using workshopdiomedes.Functions.Entities;
using workshopdiomedes.Functions.Functions;
using workshopdiomedes.Tests.Helpers;
using Xunit;

namespace workshopdiomedes.Tests.Tests
{
    public class WorkshopApiTest
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        [Fact]
        public async void CreateWorkshop_Should_Return_200()
        {
            // Arrenge
            MockCloudTableWorkshop mockWorkshop = new MockCloudTableWorkshop(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Workshop workshopRequest = TestFactory.GetWorkshopRequest();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(workshopRequest);

            // Act
            IActionResult response = await WorkshopApi.CreateWorkshop(request,mockWorkshop,logger);

            // Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void UpdateWorkshop_Should_Return_200()
        {
            // Arrenge
            MockCloudTableWorkshop mockWorkshop = new MockCloudTableWorkshop(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Workshop workshopRequest = TestFactory.GetWorkshopRequest();
            Guid workshopId = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(workshopId,workshopRequest);

            // Act
            IActionResult response = await WorkshopApi.UpdateWorkshop(request,mockWorkshop,workshopId.ToString(),logger);

            // Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        
        [Fact]
        public async void GetAllWorkshop_Should_Return_200()
        {
            // Arrenge
            MockCloudTableWorkshop mockWorkshop = new MockCloudTableWorkshop(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            DefaultHttpRequest request = TestFactory.CreateHttpRequest();

            // Act
            IActionResult response = await WorkshopApi.GetAllWorkshop(request,mockWorkshop,logger);

            // Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void DeleteWorkshop_Should_Return_200()
        {
            // Arrenge
            MockCloudTableWorkshop mockWorkshop = new MockCloudTableWorkshop(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Guid workshopId = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(workshopId);
            WorkshopEntity workshopEntity = TestFactory.GetWorkshopEntity();

            // Act
            IActionResult response = await WorkshopApi.DeleteWorkshop(request, workshopEntity,mockWorkshop,workshopId.ToString(),logger);

            // Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }


        [Fact]
        public void GetWorkshopById_Should_Return_200()
        {
            // Arrenge
            MockCloudTableWorkshop mockWorkshop = new MockCloudTableWorkshop(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Guid workshopId = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(workshopId);
            WorkshopEntity workshopEntity = TestFactory.GetWorkshopEntity();

            // Act
            IActionResult response = WorkshopApi.GetWorkshopById(request,workshopEntity,workshopId.ToString(),logger);

            // Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }


    }
}
