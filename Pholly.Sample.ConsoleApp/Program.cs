using Pholly.Sample.Shared;
using System;

namespace Pholly.Sample.App
{
    class Program
    {
        static ActivityTracker _activityTracker = new();
        static void Main(string[] args)
        {

            Console.WriteLine("Simple Activity Tracker");
            Console.WriteLine(@"AddAction(""jump"", 100)");
            Console.WriteLine(@"AddAction(""run"", 75)");
            Console.WriteLine(@"AddAction(""jump"", 200)");

            AddAction("jump", 100);
            AddAction("run", 75);
            AddAction("jump", 200);

            Console.WriteLine($"Avg time in seconds for all added actions: {_activityTracker.GetStats()}");
        }

        static void AddAction(string action, int timeInSeconds)
        {
            var result = _activityTracker.AddAction($@"{{""action"": ""{action}"", ""time"": {timeInSeconds} }}");
            if (result == null)
            {
                //do something
            }
        }
    }
}
