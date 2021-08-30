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
        [FunctionName(nameof(CreateEmployee))]
        public static async Task<IActionResult> CreateEmployee(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log)
        {
            log.LogInformation("Received a new employee.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Todo todo = JsonConvert.DeserializeObject<Todo>(requestBody);

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
                Time2Work = todo.Time2Work,
                Types = todo.Types,
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

        [FunctionName(nameof(UpdateEmployee))]
        public static async Task<IActionResult> UpdateEmployee(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")] HttpRequest req,
        [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
        string id,
        ILogger log)
        {
            log.LogInformation($"Update for employee: {id}, received.");

            //receive parameters
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Todo todo = JsonConvert.DeserializeObject<Todo>(requestBody);

            //validate todo id
            TableOperation findOperation = TableOperation.Retrieve<TodoEntity>("TODO", id);
            TableResult findResult = await todoTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "employee not found."
                });
            }

            //update todo
            TodoEntity todoEntity = (TodoEntity)findResult.Result;
            todoEntity.IdEmployee = todo.IdEmployee;
            //if (!string.IsNullOrEmpty(todo.IdEmployee))
            if (!string.IsNullOrEmpty(todo?.Types.ToString()))
            {
                todoEntity.Types = todo.Types;
            }
            if (!string.IsNullOrEmpty(todo?.Time2Work.ToString()))
            {
                todoEntity.Time2Work = todo.Time2Work;
            }

            //save the task

            TableOperation addOperation = TableOperation.Replace(todoEntity);
            await todoTable.ExecuteAsync(addOperation);

            string message = $"Employee: {id}, updated in table.";
            log.LogInformation(message);


            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = todoEntity
            });
        }

        [FunctionName(nameof(GetAllEmployees))]
        public static async Task<IActionResult> GetAllEmployees(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req,
            [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log)
        {
            log.LogInformation("Get all employees received.");

            TableQuery<TodoEntity> query = new TableQuery<TodoEntity>();
            TableQuerySegment<TodoEntity> todos = await todoTable.ExecuteQuerySegmentedAsync(query, null);

            string message = "Retrieved all employees";
            log.LogInformation(message);


            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = todos
            });
        }

        [FunctionName(nameof(GetEmployeeById))]
        public static async Task<IActionResult> GetEmployeeById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")] HttpRequest req,
            [Table("todo", "TODO", "{id}", Connection = "AzureWebJobsStorage")] TodoEntity todoEntity,
            string id,
            ILogger log)
        {
            log.LogInformation($"Get employees by id: {id}, received.");

            if (todoEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Todo not found."
                });
            }

            //string message = $"Todo : {id}, retrieved"; esto es lo mismo que lo de abajo
            string message = $"Todo : {todoEntity.RowKey}, retrieved";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = todoEntity
            });
        }

        [FunctionName(nameof(DeleteEmployee))]
        public static async Task<IActionResult> DeleteEmployee(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")] HttpRequest req,
            [Table("todo", "TODO", "{id}", Connection = "AzureWebJobsStorage")] TodoEntity todoEntity,
            [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Delete employee: {id}, received.");

            if (todoEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Employee not found."
                });
            }

            await todoTable.ExecuteAsync(TableOperation.Delete(todoEntity));
            string message = $"Todo : {todoEntity.RowKey}, deleted";
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
