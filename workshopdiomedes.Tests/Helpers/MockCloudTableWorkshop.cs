using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using workshopdiomedes.Functions.Entities;

namespace workshopdiomedes.Tests.Helpers
{
    class MockCloudTableWorkshop : CloudTable
    {
        public MockCloudTableWorkshop(Uri tableAddress) : base(tableAddress)
        {
        }

        public MockCloudTableWorkshop(Uri tableAbsoluteUri, StorageCredentials credentials) : base(tableAbsoluteUri, credentials)
        {
        }

        public MockCloudTableWorkshop(StorageUri tableAddress, StorageCredentials credentials) : base(tableAddress, credentials)
        {
        }

        public override async Task<TableQuerySegment<WorkshopEntity>> ExecuteQuerySegmentedAsync<WorkshopEntity>(TableQuery<WorkshopEntity> query, TableContinuationToken token)
        {
            ConstructorInfo constructor = typeof(TableQuerySegment<WorkshopEntity>)
                   .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                   .FirstOrDefault(c => c.GetParameters().Count() == 1);

            return await Task.FromResult(constructor.Invoke(new object[] { TestFactory.GetAllWorkshopEntity()}) as TableQuerySegment<WorkshopEntity>);
        }

        public override async Task<TableResult> ExecuteAsync(TableOperation operation)
        {
            return await Task.FromResult(new TableResult
            {
                HttpStatusCode = 200,
                Result = TestFactory.GetWorkshopEntity()
            });
        }
    }
}
