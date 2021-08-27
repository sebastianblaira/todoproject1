using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace todoproject1.Functions.Entities
{
    public class TodoEntity : TableEntity
    {
        public int IdEmployee { get; set; }

        public DateTime Time2Work { get; set; }

        public int Types { get; set; }

        public bool Consolidated { get; set; }
    }
}
