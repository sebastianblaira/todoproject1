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
            foreach (TodoEntity entryEmployee in entryEmployees)
            {
                foreach (TodoEntity exitEmployee in exitEmployees)
                {
                    if(exitEmployee.IdEmployee.Equals(entryEmployee.IdEmployee))
                    {
                        TodoEntity2 todoEntity2 = new TodoEntity2
                        {
                            IdEmployee = exitEmployee.IdEmployee,
                            TimeAllWork = DateTime.Now,
                            TimeWorked = Convert.ToInt32((exitEmployee.Time2Work - entryEmployee.Time2Work).TotalHours),
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
