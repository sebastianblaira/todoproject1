using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using todoproject1.Common.Responses;
using todoproject1.Functions.Entities;

namespace todoproject1.Functions.Functions
{
    class ConsolidatedApi
    {
        [FunctionName(nameof(GetEmployeesByDate))]
        public static async Task<IActionResult> GetEmployeesByDate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo2/{date}")] HttpRequest req,
            [Table("todo2", Connection = "AzureWebJobsStorage")] CloudTable todoTable2,
            string date,
            ILogger log)
        {
            string consolidatedEmployees = TableQuery.GenerateFilterConditionForDate("TimeAllWork", QueryComparisons.GreaterThanOrEqual, Convert.ToDateTime(date));
            TableQuery<TodoEntity2> firstQuery = new TableQuery<TodoEntity2>().Where(consolidatedEmployees);
            TableQuerySegment<TodoEntity2> secondQuery = await todoTable2.ExecuteQuerySegmentedAsync(firstQuery, null);

            if ( secondQuery == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "There aren't employees available for that date"
                });
            }

            string message = "the employees of that date are";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = secondQuery
            });
        }
    }
}
