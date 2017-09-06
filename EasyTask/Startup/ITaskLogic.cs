using EasyTask.Models;
using System;
using System.Collections.Generic;

namespace EasyTask.Startup
{
    public interface ITaskLogic
    {
        void Empty();
        void KillTask(string Id);

        void KillAllTasks();

        TaskStatusResult RetrieveTaskStatus(string Id);

        IEnumerable<dynamic> RetrieveTaskPoolInfo();

        void UpdateTask(TaskModel task);

        void DisableTask(string id);
        IEnumerable<TaskModel> GetAllTasks(bool onlyEnabled);

        void Init(string Id = null);
        void Stop();

        void Start();

        void StartSingle(string Id);

        void ExecuteTaskOnce(string Id);

        void EnsureTaskIsScheduled(Type task, TimeSpan timeBetweenTasks, string name = null, bool enabled = true,
            DateTime? startDate = null);
    }
}
