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
          [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "workshop/{id}")] HttpRequest req,
          [Table("workshop", Connection = "AzureWebJobsStorage")] CloudTable workshopTable,
          string id,
          ILogger log)
        {
            log.LogInformation($"Update for input or output:{id},received.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Workshop workshop = JsonConvert.DeserializeObject<Workshop>(requestBody);

            //Validate input or output id
            TableOperation findOperation = TableOperation.Retrieve<WorkshopEntity>("WORKSHOP", id);
            TableResult findResult = await workshopTable.ExecuteAsync(findOperation);

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


            if (!(string.IsNullOrEmpty(workshop.idemployee.ToString())) && !(string.IsNullOrEmpty(workshop.date.ToString())) && !(string.IsNullOrEmpty(workshop.type.ToString())))
            {
                workshopEntity.idemployee = workshop.idemployee;
                workshopEntity.date = workshop.date;
                workshopEntity.type = workshop.type;
            }

            TableOperation addOperation = TableOperation.Replace(workshopEntity);
            await workshopTable.ExecuteAsync(addOperation);

            string message = $"Input or output: {id}, update in table";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = workshopEntity
            });
        }

        [FunctionName(nameof(GetAllWorkshop))]
        public static async Task<IActionResult> GetAllWorkshop(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "workshop")] HttpRequest req,
           [Table("workshop", Connection = "AzureWebJobsStorage")] CloudTable workshopTable,
           ILogger log)
        {
            log.LogInformation("Get all input and output received");

            TableQuery<WorkshopEntity> query = new TableQuery<WorkshopEntity>();
            TableQuerySegment<WorkshopEntity> workshops = await workshopTable.ExecuteQuerySegmentedAsync(query, null);

            string message = "Retrieved all input and output";
            log.LogInformation(message);


            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = workshops
            });
        }

        [FunctionName(nameof(GetWorkshopById))]
        public static IActionResult GetWorkshopById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "workshop/{id}")] HttpRequest req,
            [Table("workshop", "WORKSHOP", "{id}", Connection = "AzureWebJobsStorage")] WorkshopEntity workshopEntity,
            string id,
            ILogger log)
        {
            log.LogInformation($"Get input or output by id: {id}, received.");

            if (workshopEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Todo not found."
                });
            }

            string message = $"input or output: {workshopEntity.RowKey}, retrieved.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = workshopEntity
            });
        }

        [FunctionName(nameof(DeleteWorkshop))]
        public static async Task<IActionResult> DeleteWorkshop(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "workshop/{id}")] HttpRequest req,
        [Table("workshop", "WORKSHOP", "{id}", Connection = "AzureWebJobsStorage")] WorkshopEntity workshopEntity,
        [Table("workshop", Connection = "AzureWebJobsStorage")] CloudTable workshopTable,
        string id,
        ILogger log)
        {
            log.LogInformation($"Deleyte input or output: {id}, received.");

            if (workshopEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Input or output not found."
                });
            }

            await workshopTable.ExecuteAsync(TableOperation.Delete(workshopEntity));
            string message = $"Input or output: {workshopEntity.RowKey}, deleted.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = workshopEntity
            });
        }


        [FunctionName(nameof(ConsolidateWorkshop))]
        public static async Task<IActionResult> ConsolidateWorkshop(
           [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "consolidated")] HttpRequest req,
           [Table("workshop", Connection = "AzureWebJobsStorage")] CloudTable workshopTable,
           [Table("consolidated", Connection = "AzureWebJobsStorage")] CloudTable ConsolidatedTable,
           ILogger log)
        {
            log.LogInformation("Get all input and output received");

            string filter = TableQuery.GenerateFilterConditionForBool("consolidated", QueryComparisons.Equal, false);
            TableQuery<WorkshopEntity> query = new TableQuery<WorkshopEntity>().Where(filter);
            TableQuerySegment<WorkshopEntity> workshopsFalse = await
            workshopTable.ExecuteQuerySegmentedAsync(query, null);
            int count = 0;
            string message = "";
            if (workshopsFalse.Results.Count != 0)
            {
                foreach (WorkshopEntity completedTodo2 in workshopsFalse)
                {
                    if(completedTodo2.type==0){
                    DateTime DateIn = default(DateTime);
                    DateTime DateOut = default(DateTime);
                    TimeSpan difFechas = TimeSpan.Zero;
                    int verificar = 0;

                    foreach (WorkshopEntity completedTodo in workshopsFalse)
                    {
                        if (completedTodo.idemployee == completedTodo2.idemployee)
                        {
                            if (completedTodo.type == 0)
                            {
                                verificar++;
                                DateIn = completedTodo.date;
                            }
                            else
                            {
                                verificar++;
                                DateOut = completedTodo.date;
                            }
                        }
                        completedTodo.consolidated = true;
                        TableOperation addOperation = TableOperation.Replace(completedTodo);
                        await workshopTable.ExecuteAsync(addOperation);

                    }

                    if (verificar == 2)
                    {
                        count++;
                        difFechas = DateOut - DateIn;
                        ConsolidatedEntity consolEntity = new ConsolidatedEntity
                        {
                            ETag = "*",
                            PartitionKey = "CONSOLIDATED",
                            RowKey = Guid.NewGuid().ToString(),
                            date = DateTime.Today,
                            minutesWork = (int)difFechas.TotalMinutes,
                            idemployee = completedTodo2.idemployee
                        };

                        TableOperation addOperation = TableOperation.Insert(consolEntity);
                        await ConsolidatedTable.ExecuteAsync(addOperation);
                    }
                    else
                    {
                        foreach (WorkshopEntity completedTodo in workshopsFalse)
                        {
                            if (completedTodo.idemployee == completedTodo2.idemployee)
                            {
                                if (completedTodo.type == 0)
                                {
                                    completedTodo.consolidated = false;
                                    TableOperation addOperation = TableOperation.Replace(completedTodo);
                                    await workshopTable.ExecuteAsync(addOperation);
                                }
                            }
                        }
                    }
                }
                }

                
            }

            message = $"Se añadieron {count} registros";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                //Result = workshopsFalse
            });
        }


        [FunctionName(nameof(GetConsolidateByDate))]
        public static async Task<IActionResult> GetConsolidateByDate(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "consolidated/{date}")] HttpRequest req,
          [Table("consolidated", Connection = "AzureWebJobsStorage")] CloudTable consolidateTable,
          DateTime date,
          ILogger log)
        {
            log.LogInformation($"Get for consolidated:{date},received.");

            string filter = TableQuery.GenerateFilterConditionForDate("date", QueryComparisons.Equal,date);
            TableQuery<ConsolidatedEntity> query = new TableQuery<ConsolidatedEntity>().Where(filter);
            TableQuerySegment<ConsolidatedEntity> consolidatedsByDate = await consolidateTable.ExecuteQuerySegmentedAsync(query, null);

            if (consolidatedsByDate.Results.Count==0)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "There are no consolidated for that date."
                });
            }

            string message = $"Consolidated to date: {date}";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = consolidatedsByDate
            });
        }

    }
}
