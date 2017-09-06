using System;

namespace EasyTask.Models
{
    public class TaskModel
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public int Seconds { get; set; }

        public string Type { get; set; }

        public bool Enabled { get; set; }

        public DateTime LastStart { get; set; }

        public DateTime LastEnd { get; set; }

        public DateTime LastSuccess { get; set; }

        public DateTime? NextRunTime { get; set; }
        public string LastException { get; set; }
        public DateTime? StartTaskAt { get; set; }
    }
}
