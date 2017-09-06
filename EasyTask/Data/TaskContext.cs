using EasyTask.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EasyTask.Data
{
    public class TaskContext
    {

        public static List<TaskModel> AllTasks;
        const string path = @"/Data/Json/tasks.json";

        public void AddTask(TaskModel taskModel)
        {
            LoadSet();
            AllTasks.Add(taskModel);
            Save();
        }
        public void DisableTask(string id)
        {
            LoadSet();
            var theTasks = AllTasks.Where(x => x.Id == id);
            foreach (var item in theTasks.ToList())
            {
                item.Enabled = false;
                UpdateTask(item);
            }
        }
        public void EnableTask(string id)
        {
            LoadSet();
            var theTasks = AllTasks.Where(x => x.Id == id);
            foreach (var item in theTasks.ToList())
            {
                item.Enabled = true;
                UpdateTask(item);
            }
        }
        public void UpdateTask(TaskModel taskModel)
        {
            LoadSet();
            var toChange = AllTasks.Where(x => x.Id == taskModel.Id).FirstOrDefault();
            AllTasks.Remove(toChange);
            AllTasks.Add(taskModel);
            Save();
        }
        public List<TaskModel> LoadSet()
        {
            if (AllTasks == null)
            {
                var allTasks = System.IO.File.ReadAllText(GetValidPath());
                AllTasks = JsonConvert.DeserializeObject<List<TaskModel>>(allTasks);

                if (AllTasks == null)
                {
                    AllTasks = new List<TaskModel>();
                }
            }
            return AllTasks;
        }
        public void Save()
        {
            //serialize all tasks and save
            var allTasksSerialized = JsonConvert.SerializeObject(AllTasks);
            System.IO.File.WriteAllText(GetValidPath(), allTasksSerialized);
        }
        public void ClearTasks()
        {
            AllTasks = new List<TaskModel>();
            Save();
        }

        string GetValidPath()
        {
            string currDirectory = string.Empty;

            currDirectory = AppDomain.CurrentDomain.BaseDirectory;

            //var isWebConfig = Path.GetFileName(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            ////dirty, just want to avoid a dependency on system.web...sorry
            //if (isWebConfig.ToLower() == "web.config")
            //{
            //    currDirectory = AppDomain.CurrentDomain.BaseDirectory;
            //}
            //else
            //{
            //    currDirectory = Directory.GetCurrentDirectory();
            //}

            var finalPath = Path.GetFullPath(currDirectory + path);

            var fileInfo = new FileInfo(finalPath);

            fileInfo.Directory.Create();

            if (!System.IO.File.Exists(finalPath))
            {
                var stream = File.Create(finalPath);
                stream.Close();
            }
            return finalPath;
        }
    }
}
