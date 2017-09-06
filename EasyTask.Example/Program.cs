using EasyTask.Infrastructure;
using EasyTask.Startup;
using SimpleInjector;
using System;

namespace EasyTask.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new Container();


            container.Register<Func<string, object>>(() =>
            {
                return (str) =>
                {
                    var type = System.Type.GetType(str);
                    var instance = container.GetInstance(type);

                    if (instance.GetType().IsAssignableFrom(typeof(ITask)))
                    {
                        return instance as ITask;
                    }
                    return null;
                };
            });

            var Startup = new TaskStartup(container.GetInstance<Func<string, object>>());

            Console.WriteLine("Hello World!");
        }
    }
}
