using EasyTask.Infrastructure;
using EasyTask.Models;
using System;
using System.Collections.Generic;

namespace EasyTask.Startup
{
    public class TaskStartup : ITaskStartup
    {
        /// <summary>
        /// A func to use to resolve the task types
        /// </summary>
        public Func<string, object> _resolver;
        TaskBusinessLogic _businessLogic;
        List<TaskModel> _allTasks;

        ITaskManager _taskManager;
        public TaskStartup(Func<string, object> resolver)
        {
            _resolver = resolver;
            _allTasks = new List<TaskModel>();
            _taskManager = new TaskManager(_resolver);
            _businessLogic = new TaskBusinessLogic(_taskManager);
        }

        public void AddTask(TaskModel task)
        {
            _allTasks.Add(task);
        }
        //public void DisableTask(string Id)
        //{
        //    _businessLogic.DisableTask(Id);
        //}
        //public void EnableTask(string Id)
        //{
        //    _businessLogic.EnableTask(Id);
        //}

        public void Start(bool emptyAllTasksOnStartup = false)
        {
            if (emptyAllTasksOnStartup)
            {
                _businessLogic.Empty();
            }

            foreach (var item in _allTasks)
            {
                _businessLogic.AddTask(item);
            }

            //add health checker
            var healthCheck = new Models.TaskModel
            {
                Id = "TaskHealthChecker",
                Name = "TaskHealthChecker",
                Enabled = true,
                Seconds = 2,
                Type = null,
                StartTaskAt = DateTime.Now,
                NextRunTime = DateTime.Now,
                LastEnd = DateTime.Now,
                LastStart = DateTime.Now
            };
            _businessLogic.AddTask(healthCheck);


            _taskManager.Init();

            _taskManager.Start();
        }
    }
}
