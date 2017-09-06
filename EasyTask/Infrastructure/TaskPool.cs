using System;
using System.Threading;

namespace EasyTask.Infrastructure
{
    public class TaskPool
    {
        bool disposed;

        public string Id { get; set; }

        Timer timer = null;
        /// <summary>
        /// The task that this task thread should run
        /// </summary>
        public Task Task { get; set; }
        /// <summary>
        /// How often the task should be executed, this is in minutes
        /// </summary>
        public int Interval { get; set; }

        public volatile bool IsPoolsTaskCurrentlyRunning;

        public bool IsPoolAlive { get; set; }
        public bool StartImmediate { get; set; }

        void ChangeTaskStatus(bool which)
        {
            IsPoolsTaskCurrentlyRunning = which;
        }

        public virtual void SetTimer(Action<Task> startTaskCallback, Action<Task> endTaskCallback)
        {
            this.timer = new Timer((cb) =>
            {
                //dont overlap tasks
                if (!IsPoolsTaskCurrentlyRunning)
                {
                    //show that this thread is running
                    ChangeTaskStatus(true);
                    startTaskCallback(Task);
                    timer.Change(-1, -1);
                    Task.Execute();
                    ChangeTaskStatus(false);

                    timer.Change(this.Interval, this.Interval);

                    endTaskCallback(Task);
                }
            }, null, StartImmediate ? 0 : this.Interval, this.Interval);
        }

        public void ExecuteOnce(Action<Task> startTaskCallback, Action<Task> endTaskCallback)
        {
            //dont overlap tasks
            if (!IsPoolsTaskCurrentlyRunning)
            {
                //show that this thread is running
                ChangeTaskStatus(true);
                startTaskCallback(Task);
                Task.Execute();
                
                ChangeTaskStatus(false);
                endTaskCallback(Task);
            }
        }
        public void AddTaskToThread(Task task)
        {
            this.Task = task;
        }

        public void Dispose()
        {
            if (timer != null && !disposed)
                lock (this)
                {
                    this.timer.Dispose();
                    this.timer = null;
                    this.disposed = true;
                    IsPoolAlive = false;
                }
        }

    }
}
