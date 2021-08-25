using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace todoproject1.Functions.Entities
{
    public class TodoEntity : TableEntity
    {
        public int IdEmployee { get; set; }

        public DateTime EntryTime { get; set; }

        public DateTime OutTime { get; set; }

        public char Type { get; set; }

        public bool Consolidated { get; set; }
    }
}
