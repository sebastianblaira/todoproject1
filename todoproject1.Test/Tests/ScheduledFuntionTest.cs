using System;
using System.Collections.Generic;
using System.Text;
using todoproject1.Functions.Functions;
using todoproject1.Test.Helpers;
using Xunit;

namespace todoproject1.Test.Tests
{
    public class ScheduledFuntionTest
    {
        [Fact]
        public void ScheduledFuntion_Should_Log_Message()
        {
            //Arrange
            MockCloudTableTodos mockTodos = new MockCloudTableTodos(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            ListLogger logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);
            //Add
            ScheduledFunction.Run(null, null, mockTodos, logger);
            string message = logger.Logs[0];
            //Assert
            Assert.Contains("Deleting completed", message);
        }
    }
}
