using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using todoproject1.Common.Models;
using todoproject1.Common.Responses;
using todoproject1.Functions.Entities;

namespace todoproject1.Functions.Functions
{
    public static class TodoApi
    {
        [FunctionName(nameof(CreateTodo))]
        public static async Task<IActionResult> CreateTodo(
            //first request
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log)
        {
            log.LogInformation("Recieved a new todo.");

            string name = req.Query["name"];

            //to read all
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //convert body to task
            //to read body message
            Todo todo = JsonConvert.DeserializeObject<Todo>(requestBody);

            //if the task dont have a description

            if (string.IsNullOrEmpty(todo?.IdEmployee.ToString()))
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "The request must be have a IdEmployee."
                });
            }

            //create an entry to the table todo
            TodoEntity todoEntity = new TodoEntity
            {
                IdEmployee = todo.IdEmployee,
                Time2Work = DateTime.UtcNow,
                Types = 1,
                Consolidated = false,
                ETag = "*",
                PartitionKey = "TODO",
                RowKey = Guid.NewGuid().ToString()//table index, no repeat
            };

            //load entity
            TableOperation addOperation = TableOperation.Insert(todoEntity);
            await todoTable.ExecuteAsync(addOperation);

            string message = "New employee stored in table";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = todoEntity
            });
        }
    }
}
