using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using todoproject1.Common.Models;
using todoproject1.Functions.Entities;
using todoproject1.Functions.Functions;
using todoproject1.Test.Helpers;
using Xunit;

namespace todoproject1.Test.Tests
{
    public class TodoApiTest
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        [Fact]
        public async void CreateEmployee_Should_Return_200()
        {
            // Arrenge
            MockCloudTableTodos mockEmployee = new MockCloudTableTodos(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Todo EmployeeRequest = TestFactory.GetTodoRequest();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(EmployeeRequest);
            // Act
            IActionResult response = await TodoApi.CreateEmployee(request, mockEmployee, logger);
            // Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void UpdateEmployee_Should_Return_200()
        {
            // Arrenge
            MockCloudTableTodos mockEmployee = new MockCloudTableTodos(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Todo EmployeeRequest = TestFactory.GetTodoRequest();
            Guid EmployeeId = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(EmployeeId, EmployeeRequest);
            // Act
            IActionResult response = await TodoApi.UpdateEmployee(request, mockEmployee, EmployeeId.ToString(), logger);
            // Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void GetAllEmployees_Should_Return_200()
        {
            // Arrenge
            MockCloudTableTodos mockEmployee = new MockCloudTableTodos(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Todo EmployeeRequest = TestFactory.GetTodoRequest();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(EmployeeRequest);
            // Act
            IActionResult response = await TodoApi.GetAllEmployees(request, mockEmployee, logger);
            // Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void DelEmployeeById_Should_Return_200()
        {
            // Arrenge
            MockCloudTableTodos mockEmployee = new MockCloudTableTodos(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Todo EmployeeRequest = TestFactory.GetTodoRequest();
            Guid EmployeeId = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(EmployeeId, EmployeeRequest);
            // Act
            IActionResult response = await TodoApi.GetEmployeeById(request,  mockEmployee, EmployeeId.ToString(), logger);
            // Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }
    }
}
