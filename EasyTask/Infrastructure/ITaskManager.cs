using System.Collections.Generic;

namespace EasyTask.Infrastructure
{

    public interface ITaskManager
    {
        void Init(string Id = null);

        void Stop();

        void Start(string Id = null);

        IList<TaskPool> Threads();

        void ExecuteTaskOnce(string Id);
    }
}
