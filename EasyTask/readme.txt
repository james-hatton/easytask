Create your task class and implement ITask: 

	internal class Test : ITask
    {
        public TaskModel CurrentTask { get; set; }

        public bool Execute()
        {
			var taskStatus = true;
            Console.WriteLine($"Running: "{CurrentTask.Name});
            return taskStatus;
        }

        public void Status(TaskStatusResult result)
        {
            throw new System.NotImplementedException();
        }
    }



To start the task manager.


//provide somewhere for the task manager to retrieve instances of your tasks, from a container etc...
var _resolver = container.GetInstance<Func<string, object>>();

var taskStartup = new TaskStartup(_resolver);
            taskStartup.AddTask(new Halsbury.Taskmanager.Models.TaskModel
            {
                Id = "taskone-needstobeunique",
                Enabled = true,
                Name = "Task one",
                Seconds = 3,
                Type = "ConsoleApp1.Test, ConsoleApp1"
            });

            taskStartup.AddTask(new Halsbury.Taskmanager.Models.TaskModel
            {
                Id = "tasktwo-needstobeunique",
                Enabled = true,
                Name = "Task two",
                Seconds = 3,
                Type = "ConsoleApp1.Test2, ConsoleApp1"
            });

            taskStartup.Start();


//Things to be aware of.
Ensure the id for each task is unique and don't change id's once you have already run the task manager.

The enabled flag can be changed to true or false to enable or disable a task.
