
using EasyTask.Data;
using EasyTask.Models;
using EasyTask.Startup;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTask.Infrastructure
{
    public class TaskBusinessLogic : ITaskLogic
    {
        readonly ITaskManager _taskManager;

        public TaskContext TaskContext { get; }

        public TaskBusinessLogic(ITaskManager taskmanager)
        {
            _taskManager = taskmanager;
            TaskContext = new TaskContext();
        }

        public void EnsureTaskIsScheduled(Type task,
            TimeSpan timeBetweenTasks,
            string name = null,
            bool enabled = true,
            DateTime? startDate = null)
        {
            if (name == null)
            {
                name = task.AssemblyQualifiedName;
            }

            if (!task.GetInterfaces().Contains(typeof(ITask)))
            {
                throw new ArgumentException("Type does not inherit ITask", nameof(task));
            }

            var seconds = (int)timeBetweenTasks.TotalSeconds;
            if (seconds < 20)
            {
                throw new ArgumentException("Timespan must be more than 20 seconds", nameof(timeBetweenTasks));
            }

            var newTask = new TaskModel()
            {
                Enabled = enabled,
                Type = task.AssemblyQualifiedName,
                Name = name,
                StartTaskAt = startDate,
                Seconds = seconds,
                LastStart = DateTime.Now,
                LastEnd = DateTime.Now,
                LastSuccess = DateTime.Now
            };

            var dbTask = TaskContext.LoadSet().FirstOrDefault(x => x.Name == name);
            var shouldRemove = dbTask != null && dbTask.Seconds != seconds;

            if (shouldRemove)
            {
                TaskContext.LoadSet().Remove(dbTask);
            }
            if (dbTask == null || shouldRemove)
            {
                TaskContext.LoadSet().Add(newTask);
            }

            TaskContext.Save();
        }
        public void DisableTask(string id)
        {
            TaskContext.DisableTask(id);
        }
        public void EnableTask(string id)
        {
            TaskContext.EnableTask(id);
        }
        public void KillTask(string Id)
        {
            var pools = _taskManager.Threads();
            var task = pools.First(x => x.Id == Id);
            //kill the task
            task.Dispose();

            _taskManager.Threads().Remove(task);
            task = null;
        }
        public void KillAllTasks()
        {
            var pools = _taskManager.Threads();
            var task = pools.All((t) =>
            {
                t.Dispose();
                t = null;
                return true;
            });
            pools.Clear();
        }
        public TaskStatusResult RetrieveTaskStatus(string Id)
        {
            var pools = _taskManager.Threads();
            var task = pools.First(x => x.Id == Id);

            return task.Task.Status;
        }
        public IEnumerable<dynamic> RetrieveTaskPoolInfo()
        {
            var pools = _taskManager.Threads();

            return pools;
        }

        public void AddTask(TaskModel task)
        {
            var context = new TaskContext();

            var allTasks = context.LoadSet();

            var theTask = allTasks.FirstOrDefault(x => x.Name == task.Name);
            if (theTask == null)
            {
                var ourNewTask = new TaskModel
                {
                    Id = task.Id,
                    Enabled = task.Enabled,
                    Name = task.Name,
                    Type = task.Type,
                    StartTaskAt = task.StartTaskAt,
                    Seconds = task.Seconds,
                    NextRunTime = task.NextRunTime,
                    LastEnd = task.LastEnd == DateTime.MinValue ? DateTime.Now : task.LastEnd,
                    LastStart = task.LastStart == DateTime.MinValue ? DateTime.Now : task.LastStart,
                    LastSuccess = task.LastSuccess == DateTime.MinValue ? DateTime.Now : task.LastSuccess
                };
                allTasks.Add(ourNewTask);
                theTask = ourNewTask;
            }

            //has the time been changed? 
            if (theTask.Seconds != task.Seconds)
            {
                theTask.Seconds = task.Seconds;
                context.UpdateTask(theTask);
            }

            if (theTask.Enabled != task.Enabled)
            {
                theTask.Enabled = task.Enabled;
                context.UpdateTask(theTask);
            }

            context.Save();
        }
        public void UpdateTask(TaskModel task)
        {
            var t = TaskContext.LoadSet().FirstOrDefault(x => x.Type == task.Type);

            t.Enabled = task.Enabled;
            t.LastEnd = task.LastEnd;
            t.LastStart = task.LastStart;
            t.LastSuccess = task.LastSuccess;
            t.Name = task.Name;
            t.Seconds = task.Seconds;
            t.Type = task.Type;
            t.LastException = task.LastException;
            t.NextRunTime = task.NextRunTime;
            TaskContext.UpdateTask(t);
        }

        public virtual IEnumerable<TaskModel> GetAllTasks(bool onlyEnabled)
        {
            var tasks = TaskContext.LoadSet().Select(x => new TaskModel()
            {
                Enabled = x.Enabled,
                LastEnd = x.LastEnd,
                LastStart = x.LastStart,
                LastSuccess = x.LastSuccess,
                Name = x.Name,
                Seconds = x.Seconds,
                Type = x.Type,
                LastException = x.LastException,
                Id = x.Id,
                NextRunTime = x.NextRunTime,
                StartTaskAt = x.StartTaskAt
            }).ToList();

            if (onlyEnabled)
            {
                return tasks.Where(x => x.Enabled && (x.StartTaskAt <= DateTime.Now || x.StartTaskAt == null));
            }
            return tasks;
        }

        public void Stop()
        {
            _taskManager.Stop();
        }

        public void Empty()
        {
            TaskContext.ClearTasks();
        }
        public void Start()
        {
            _taskManager.Start();
        }
        public void StartSingle(string Id)
        {
            _taskManager.Init(Id);
            _taskManager.Start(Id);
        }
        public void ExecuteTaskOnce(string Id)
        {
            _taskManager.ExecuteTaskOnce(Id);
        }

        public void Init(string Id)
        {
            _taskManager.Init(Id);
        }
        void CheckConnection()
        {

        }
    }
}
