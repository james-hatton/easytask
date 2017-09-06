using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTask.Infrastructure
{
    public class TaskManager : ITaskManager
    {
        public List<TaskPool> _tasks = new List<TaskPool>();

        Func<string, object> _resolve;

        public TaskManager(Func<string, object> resolve)
        {
            this._resolve = resolve;
        }
        /// <summary>
        /// Starts the task manager
        /// </summary>
        public void Init(string Id)
        {
            try
            {
                if (Id == null)
                {
                    this._tasks.Clear();
                }
                var _taskLogic = new TaskBusinessLogic(this);

                var allTasks = Id == null ? _taskLogic.GetAllTasks(true) : _taskLogic.GetAllTasks(true).Where(x => x.Id == Id);


                foreach (var st in allTasks)
                {
                    //does this task already exist in the pool??
                    var temptask = _tasks.FirstOrDefault(x => x.Id == st.Id);
                    if (temptask != null)
                    {
                        if (temptask.IsPoolsTaskCurrentlyRunning)
                        {
                            continue;
                        }
                    }

                    var taskPool = new TaskPool();
                    //we need to convert this to ms for the timer, it's just in seconds in the db to be more human readable.
                    taskPool.Interval = (st.Seconds * 1000);
                    taskPool.IsPoolAlive = true;
                    taskPool.Id = st.Id;

                    var task = new Task(st);

                    try
                    {
                        if (task.Name == "TaskHealthChecker")
                        {
                            task.Instance = new TaskHealthChecker(new TaskBusinessLogic(this));
                        }
                        else
                        {
                            task.Instance = _resolve(st.Type);
                        }
                        taskPool.AddTaskToThread(task);
                        _tasks.Add(taskPool);
                    }
                    catch (Exception e)
                    {
                        var ex = e;
                    }
                }
            }
            catch (Exception ex)
            {
                var e = ex;
            }
        }

        public void Stop()
        {
            var pools = _tasks;
            pools.All((t) =>
            {
                t.Dispose();
                t = null;
                return true;
            });
            _tasks.Clear();
        }
        public void ExecuteTaskOnce(string Id)
        {
            var taskPool = _tasks.Where(x => x.Id == Id).FirstOrDefault();
            if (taskPool == null)
            {
                return;
            }
            taskPool.ExecuteOnce((t) =>
                {
                    var _taskLogic = new TaskBusinessLogic(this);
                    var task = _taskLogic.GetAllTasks(true).Where(x => x.Type == t.Type).FirstOrDefault();
                    var start = DateTime.Now;
                    //new instance each time.
                    t.Instance = _resolve(task.Type);
                    task.LastStart = start;
                    task.NextRunTime = start.AddMilliseconds(taskPool.Interval);
                    _taskLogic.UpdateTask(task);
                },
                    (t) =>
                    {
                        var _taskLogic = new TaskBusinessLogic(this);
                        var task = _taskLogic.GetAllTasks(true).Where(x => x.Type == t.Type).FirstOrDefault();
                        var endTime = DateTime.Now;
                        task.LastSuccess = t.Status.Successful ? endTime : task.LastSuccess;
                        task.LastEnd = endTime;
                        if (task.LastSuccess != task.LastEnd)
                        {
                            task.LastException = t.Status.Exception.ToString();
                        }
                        taskPool.StartImmediate = false;
                        _taskLogic.UpdateTask(task);
                    });
        }
        public virtual void Start(string Id)
        {
            try
            {
                var tasks = Id == null ? _tasks : _tasks.Where(x => x.Id == Id);

                foreach (var taskThread in tasks)
                {
                    if (taskThread.IsPoolsTaskCurrentlyRunning)
                    {
                        continue;
                    }

                    if (Id != null)
                    {
                        taskThread.StartImmediate = true;
                    }

                    if (taskThread.Task.Name == "TaskHealthChecker")
                    {
                        taskThread.SetTimer((t) =>
                        {
                            var _taskLogic = new TaskBusinessLogic(this);
                            var task = _taskLogic.GetAllTasks(true).Where(x => x.Type == t.Type).FirstOrDefault();
                            var start = DateTime.Now;
                            task.LastStart = start;
                            task.NextRunTime = start.AddMilliseconds(taskThread.Interval);
                            _taskLogic.UpdateTask(task);
                        },
                            (t) =>
                            {
                                var _taskLogic = new TaskBusinessLogic(this);
                                var task = _taskLogic.GetAllTasks(true).Where(x => x.Type == t.Type).FirstOrDefault();
                                var endTime = DateTime.Now;
                                task.LastSuccess = t.Status.Successful ? endTime : task.LastSuccess;
                                task.LastEnd = endTime;
                                if (task.LastSuccess != task.LastEnd)
                                {
                                    task.LastException = t.Status.Exception.ToString();
                                }
                                taskThread.StartImmediate = false;
                                _taskLogic.UpdateTask(task);
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                var e = ex;
            }
        }

        public IList<TaskPool> Threads()
        {
            return _tasks;
        }
    }
}
