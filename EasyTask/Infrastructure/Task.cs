
using EasyTask.Models;
using Newtonsoft.Json;
using System;

namespace EasyTask.Infrastructure
{
    public class Task
    {
        TaskModel _taskModel;
        public Task(TaskModel scheduledTask)
        {
            this.Name = scheduledTask.Name;
            this.Enabled = scheduledTask.Enabled;
            this.Type = scheduledTask.Type;
            this._taskModel = scheduledTask;
        }

        internal void Execute()
        {
            var instance = this.Instance as ITask;
            instance.CurrentTask = _taskModel;
            if (instance != null)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();

                this.Status = new TaskStatusResult();

                Status.Started = DateTime.Now;
                Status.IsStarted = true;

                bool taskResult = false;
                try
                {
                    taskResult = instance.Execute();
                }
                catch (Exception ex)
                {
                    taskResult = false;
                    Status.Successful = false;
                    Status.Exception = ex;
                }

                watch.Stop();
                DateTime timeEnd = DateTime.Now;

                Status.Finished = timeEnd;
                Status.Successful = taskResult;
                double elapsedTime = watch.ElapsedMilliseconds;
                Status.TotalSeconds = Math.Round((elapsedTime / 1000), 2);

                try
                {
                    instance.Status(Status);
                }
                catch (Exception ex)
                {
                    taskResult = false;
                    Status.Successful = false;
                    Status.Exception = ex;
                }
            }
        }
        [JsonIgnore]
        public object Instance { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        public bool Enabled { get; set; }

        public TaskStatusResult Status { get; set; }

        public bool StartImmediate { get; set; }
        public string Id { get; internal set; }
    }
}
