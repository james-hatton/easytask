using System;

namespace EasyTask.Models
{
    public class TaskStatusResult
    {
        public bool Successful { get; set; }

        public DateTime Started { get; set; }

        public DateTime Finished { get; set; }

        public double TotalSeconds { get; set; }

        public Exception Exception { get; set; }

        public bool IsStarted { get; set; }

    }
}
