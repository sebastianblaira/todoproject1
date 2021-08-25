using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace todoproject1.Functions.Entities
{
    public class TodoEntity2 : TableEntity
    {
        public int IdEmployee { get; set; }

        public DateTime TimeAllWork { get; set; }

        public TimeSpan TimeWorked { get; set; }
    }
}
