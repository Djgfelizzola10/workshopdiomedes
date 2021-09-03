using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using workshopdiomedes.Functions.Entities;

namespace workshopdiomedes.Functions.Functions
{
    public static class ScheduledFunction
    {
        [FunctionName("ScheduledFunction")]
        public static async Task Run(
            [TimerTrigger("0 */59 * * * *")]TimerInfo myTimer,
            [Table("workshop", Connection = "AzureWebJobsStorage")] CloudTable workshopTable,
            [Table("consolidated", Connection = "AzureWebJobsStorage")] CloudTable ConsolidatedTable,
            ILogger log)
        {
            log.LogInformation($"Consolidated function executed at: {DateTime.Now}");

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

            message = $"Se añadieron: {count} registros y se actualizaron: {count2}";
            log.LogInformation(message);

        }
    }
}
