using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using workshopdiomedes.Common.Models;
using workshopdiomedes.Common.Responses;
using workshopdiomedes.Functions.Entities;

namespace workshopdiomedes.Functions.Functions
{
    public static class WorkshopApi
    {
        [FunctionName(nameof(CreateWorkshop))]
        public static async Task<IActionResult> CreateWorkshop(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "workshop")] HttpRequest req,
            [Table("workshop",Connection ="AzureWebJobsStorage") ]CloudTable workshopTable,
            ILogger log)
        {
            log.LogInformation("I received a new entry or exit.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Workshop workshop = JsonConvert.DeserializeObject<Workshop>(requestBody);

            if (string.IsNullOrEmpty(workshop?.idemployee.ToString()) || string.IsNullOrEmpty(workshop?.date.ToString()) || string.IsNullOrEmpty(workshop?.type.ToString())) {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Identification, type and date required"
                });
            }

            WorkshopEntity workshopEntity = new WorkshopEntity
            {
                ETag = "*",
                consolidated = false,
                PartitionKey = "WORKSHOP",
                RowKey = Guid.NewGuid().ToString(),
                type = workshop.type,
                date=workshop.date,
                idemployee=workshop.idemployee
            };

            TableOperation addOperation = TableOperation.Insert(workshopEntity);
            await workshopTable.ExecuteAsync(addOperation);

            string message = "New input or output added in table";
            log.LogInformation(message);

           
            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = workshopEntity
            });
        }


        [FunctionName(nameof(UpdateWorkshop))]
        public static async Task<IActionResult> UpdateWorkshop(
          [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")] HttpRequest req,
          [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
          string id,
          ILogger log)
        {
            log.LogInformation($"Update for input or output:{id},received.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Workshop workshop = JsonConvert.DeserializeObject<Workshop>(requestBody);

            //Validate input or output id
            TableOperation findOperation = TableOperation.Retrieve<WorkshopEntity>("WORKSHOP", id);
            TableResult findResult = await todoTable.ExecuteAsync(findOperation);

            if (findResult.Result == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Input or output not found."
                });
            }

            //Update input or output
            WorkshopEntity workshopEntity = (WorkshopEntity)findResult.Result;
            workshopEntity.consolidated = workshop.consolidated;
            if (!string.IsNullOrEmpty(workshop?.idemployee.ToString()))
            {
                workshopEntity.idemployee = workshop.idemployee;
            }else if (!string.IsNullOrEmpty(workshop?.date.ToString()))
            {
                workshopEntity.date = workshop.date;
            }else if (!string.IsNullOrEmpty(workshop?.type.ToString()))
            {
                workshopEntity.type = workshop.type;
            }

            TableOperation addOperation = TableOperation.Replace(workshopEntity);
            await todoTable.ExecuteAsync(addOperation);

            string message = $"Input or output: {id}, update in table";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = workshopEntity
            });
        }
    }
}
