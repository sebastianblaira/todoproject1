using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using todoproject1.Functions.Entities;

namespace todoproject1.Functions.Functions
{
    public static class ScheduledFunction
    {
        [FunctionName("ScheduledFunction")]
        public static async Task Run(
            [TimerTrigger("0 */1 * * * *")] TimerInfo myTimer,
            [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            [Table("todo2", Connection = "AzureWebJobsStorage")] CloudTable todoTable2,
            ILogger log)
        {
            log.LogInformation($"Number of hours worked per employee completed function executed at: {DateTime.Now}");

            //find (not) consolidated
            string filter = TableQuery.GenerateFilterConditionForBool("Consolidated", QueryComparisons.Equal, false);
            string entryFilter = TableQuery.GenerateFilterConditionForInt("Types", QueryComparisons.Equal, 0);
            string exitFilter = TableQuery.GenerateFilterConditionForInt("Types", QueryComparisons.Equal, 1);

            // find entry time
            TableQuery<TodoEntity> entryQuery = new TableQuery<TodoEntity>().Where(TableQuery.CombineFilters(entryFilter, TableOperators.And, filter));
            TableQuerySegment<TodoEntity> entryEmployees = await todoTable.ExecuteQuerySegmentedAsync(entryQuery, null);

            // find exit time
            TableQuery<TodoEntity> exitQuery = new TableQuery<TodoEntity>().Where(TableQuery.CombineFilters(exitFilter, TableOperators.And, filter));
            TableQuerySegment<TodoEntity> exitEmployees = await todoTable.ExecuteQuerySegmentedAsync(exitQuery, null);
            int countConsolidated = 0;
            int exist = 0;
            foreach (TodoEntity entryEmployee in entryEmployees)
            {
                foreach (TodoEntity exitEmployee in exitEmployees)
                {
                    if(exitEmployee.IdEmployee.Equals(entryEmployee.IdEmployee))
                    {
                        //validate if the employee exist in the table consolidated
                        string filterConsolidated = TableQuery.GenerateFilterConditionForInt("IdEmployee", QueryComparisons.Equal, exitEmployee.IdEmployee);
                        TableQuery<TodoEntity2> filterConsolidated2 = new TableQuery<TodoEntity2>().Where((filterConsolidated));
                        TableQuerySegment<TodoEntity2> consolidatedQuery = await todoTable2.ExecuteQuerySegmentedAsync(filterConsolidated2, null);
                        foreach (TodoEntity2 existConsolidate in consolidatedQuery)
                        {
                            if (exitEmployee.IdEmployee.Equals(existConsolidate.IdEmployee))
                            {
                                TableOperation findOperation = TableOperation.Retrieve<TodoEntity2>("TODO2", existConsolidate.RowKey);
                                TableResult findResult = await todoTable2.ExecuteAsync(findOperation);
                                TodoEntity2 todoConsolidated = (TodoEntity2)findResult.Result;

                                todoConsolidated.TimeAllWork = exitEmployee.Time2Work;
                                todoConsolidated.TimeWorked = Convert.ToInt32((((existConsolidate.TimeAllWork).Minute) + (exitEmployee.Time2Work - entryEmployee.Time2Work).TotalMinutes));

                                TableOperation addOperation = TableOperation.Replace(todoConsolidated);
                                await todoTable2.ExecuteAsync(addOperation);
                                exist = 1;
                                countConsolidated++;
                                await UpdateTable1(todoTable, entryEmployee.RowKey);
                                await UpdateTable1(todoTable, exitEmployee.RowKey);
                                break;
                            }
                            else
                            {
                                exist = 0;
                            }
                        }
                        if(exist == 0)
                        {
                            TodoEntity2 todoEntity2 = new TodoEntity2
                            {
                                IdEmployee = exitEmployee.IdEmployee,
                                TimeAllWork = exitEmployee.Time2Work,
                                TimeWorked = Convert.ToInt32((exitEmployee.Time2Work - entryEmployee.Time2Work).TotalMinutes),
                                ETag = "*",
                                PartitionKey = "TODO2",
                                RowKey = Guid.NewGuid().ToString()//table index, no repeat
                            };
                            countConsolidated++;
                            TableOperation insertEmployees = TableOperation.Insert(todoEntity2);
                            await todoTable2.ExecuteAsync((insertEmployees));
                            //put consolidated in true 
                            await UpdateTable1(todoTable, entryEmployee.RowKey);
                            await UpdateTable1(todoTable, exitEmployee.RowKey);
                        }
                    } 
                }
            }
            log.LogInformation($"Add to consolidated: {countConsolidated} employees at: {DateTime.Now}");
        }
        public static async Task UpdateTable1(
            [Table("todo", "{id}", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            string id)
        {
            TableOperation findOperation = TableOperation.Retrieve<TodoEntity>("TODO", id);
            TableResult findResult = await todoTable.ExecuteAsync(findOperation);

            //update consolidated state at table1 (todo)
            TodoEntity todoEntity = (TodoEntity)findResult.Result;
            todoEntity.Consolidated = true;

            TableOperation addOperation = TableOperation.Replace(todoEntity);
            await todoTable.ExecuteAsync(addOperation);
        }
    }
}
