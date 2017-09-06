using EasyTask.Models;
using EasyTask.Startup;
using System;
using System.Linq;

namespace EasyTask.Infrastructure
{
    public class TaskHealthChecker : ITask
    {
        ITaskLogic _taskLogic;
        public TaskHealthChecker(ITaskLogic taskLogic)
        {
            _taskLogic = taskLogic;
        }

        public TaskModel CurrentTask { get; set; }

        public bool Execute()
        {
            //There is a chance that if something happens, server restarts, app pool recycles, crashes etc...that 
            //a task that has a long time in between doesn't get run, this task
            //is a sort of 'sync' task and will start up any tasks that should have been run that have not.

            //get all enabled tasks
            var allEnabledTasks = _taskLogic.GetAllTasks(true).ToList();

            allEnabledTasks.ForEach((t) =>
            {
                var startTime = t.LastStart;

                var interval = t.Seconds;

                var nextRunTimeForTask = t.NextRunTime;

                var timeShouldRun = startTime.AddSeconds(interval);

                if (nextRunTimeForTask == null)
                {
                    //task hasn't been run yet, should it have?
                    if (DateTime.Now > timeShouldRun)
                    {
                        //yes it should
                        // _taskLogic.StartSingle(t.Id);
                        _taskLogic.ExecuteTaskOnce(t.Id);
                    }
                }
                else
                {
                    //next run time in db isn't null, is the next run time less than current time?
                    //this will catch tasks that were stopped abruptly, such as with an app pool recycle/crash etc..
                    if (nextRunTimeForTask < DateTime.Now)
                    {
                        _taskLogic.ExecuteTaskOnce(t.Id);
                        // _taskLogic.StartSingle(t.Id);
                    }
                }
            }
            );

            //success
            return true;
        }
        public void Status(TaskStatusResult result)
        {

        }
    }
}
