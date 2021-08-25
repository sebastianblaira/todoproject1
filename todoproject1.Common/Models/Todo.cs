using System;

namespace todoproject1.Common.Models
{
    public class Todo
    {
        public int IdEmployee { get; set; }

        public DateTime EntryTime { get; set; }

        public DateTime OutTime { get; set; }

        public char Type { get; set; }

        public bool Consolidated { get; set; }
    }
}
