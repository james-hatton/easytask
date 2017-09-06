

using EasyTask.Models;

namespace EasyTask.Infrastructure
{
    public interface ITask
    {
        TaskModel CurrentTask { get; set; }
        /// <summary>
        /// Executes the task, return a boolean value on whether the task succeeds or not
        /// </summary>
        /// <returns></returns>
        bool Execute();

        /// <summary>
        /// This method will pass the result of the task to you for you to do what you want with it.
        /// </summary>
        /// <param name="result"></param>
        void Status(TaskStatusResult result);
    }
}