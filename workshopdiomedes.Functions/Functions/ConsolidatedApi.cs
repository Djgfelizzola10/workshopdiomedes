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
using System.Globalization;

namespace workshopdiomedes.Functions.Functions
{
    public static class ConsolidatedApi
    {

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
                    int count2 = 0;
                    string message = "";
                    if (workshopsFalse.Results.Count != 0)
                    {
                        foreach (WorkshopEntity workshopSw in workshopsFalse)
                        {
                            if (workshopSw.type == 0)
                            {
                                DateTime DateIn = default(DateTime);
                                DateTime DateOut = default(DateTime);
                                TimeSpan difFechas = TimeSpan.Zero;
                                int verificar = 0;

                                foreach (WorkshopEntity workshopSw2 in workshopsFalse)
                                {
                                    if (workshopSw2.idemployee == workshopSw.idemployee)
                                    {
                                        if (workshopSw2.type == 0)
                                        {
                                            verificar++;
                                            DateIn = workshopSw2.date;
                                        }
                                        else
                                        {
                                            verificar++;
                                            DateOut = workshopSw2.date;
                                        }
                                    }
                                    workshopSw2.consolidated = true;
                                    TableOperation addOperation = TableOperation.Replace(workshopSw2);
                                    await workshopTable.ExecuteAsync(addOperation);

                                }

                                if (verificar == 2)
                                {
                                    string filter2 = TableQuery.CombineFilters(TableQuery.GenerateFilterConditionForInt("idemployee", QueryComparisons.Equal, workshopSw.idemployee),
                                        TableOperators.And,
                                        TableQuery.GenerateFilterConditionForDate("date", QueryComparisons.Equal, DateTime.ParseExact(workshopSw.date.ToLocalTime().ToString("yyyy-MM-dd"), "yyyy-MM-dd", CultureInfo.InvariantCulture)));
                                    TableQuery<ConsolidatedEntity> query2 = new TableQuery<ConsolidatedEntity>().Where(filter2);
                                    TableQuerySegment<ConsolidatedEntity> Consolidated2 = await ConsolidatedTable.ExecuteQuerySegmentedAsync(query2, null);
                                    if (Consolidated2.Results.Count != 0)
                                    {
                                        foreach (ConsolidatedEntity consolidatedsw in Consolidated2)
                                        {
                                            count2++;
                                            difFechas = DateOut - DateIn;
                                            consolidatedsw.minutesWork = consolidatedsw.minutesWork + (int)difFechas.TotalMinutes;
                                            TableOperation addOperation = TableOperation.Replace(consolidatedsw);
                                            await ConsolidatedTable.ExecuteAsync(addOperation);
                                        }
                                    }
                                    else
                                    {
                                        count++;
                                        difFechas = DateOut - DateIn;
                                        ConsolidatedEntity consolEntity = new ConsolidatedEntity
                                        {
                                            ETag = "*",
                                            PartitionKey = "CONSOLIDATED",
                                            RowKey = Guid.NewGuid().ToString(),
                                            date = DateTime.ParseExact(workshopSw.date.ToLocalTime().ToString("yyyy-MM-dd"), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                                            minutesWork = (int)difFechas.TotalMinutes,
                                            idemployee = workshopSw.idemployee
                                        };

                                        TableOperation addOperation = TableOperation.Insert(consolEntity);
                                        await ConsolidatedTable.ExecuteAsync(addOperation);
                                    }
                                }
                                else
                                {
                                    foreach (WorkshopEntity completedTodo in workshopsFalse)
                                    {
                                        if (completedTodo.idemployee == workshopSw.idemployee)
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

                    message = $"Consolidation sumary. Records added: {count} Records update:: {count2}";
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

            string filter = TableQuery.GenerateFilterConditionForDate("date", QueryComparisons.Equal, date);
            TableQuery<ConsolidatedEntity> query = new TableQuery<ConsolidatedEntity>().Where(filter);
            TableQuerySegment<ConsolidatedEntity> consolidatedsByDate = await consolidateTable.ExecuteQuerySegmentedAsync(query, null);

            if (consolidatedsByDate.Results.Count == 0)
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
