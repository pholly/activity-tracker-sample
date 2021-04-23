using Pholly.Sample.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Pholly.Sample.xUnitTests
{
    class ActivityStat
    {
        public string Action { get; set; }
        public int Avg { get; set; }
    }

    public class ActivityTrackerTests
    {
        JsonSerializerOptions jsonOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

        [Fact]
        public void VerifyConsecutiveActivitiesAndSingleStatPerActivity()
        {
            var activityTracker = new ActivityTracker();
            AddAction(activityTracker, "jump", 100);
            AddAction(activityTracker, "run", 75);
            AddAction(activityTracker, "jump", 200);
            var jsonStats = activityTracker.GetStats();
            var stats = JsonSerializer.Deserialize<IEnumerable<ActivityStat>>(jsonStats, jsonOptions);
            //.Single will throw an exception if there is more than one
            var jumpStat = stats.Single(s => s.Action == "jump");
            var runStat = stats.Single(s => s.Action == "run");
            Assert.Equal(150, jumpStat.Avg);
            Assert.Equal(75, runStat.Avg);
        }

        [Fact]
        public void VerifyParallelActivitiesAndSingleStatPerActivity()
        {
            var activityTracker = new ActivityTracker();
            var actions = new List<Activity>()
            {
               new Activity(){Action = "jump", Time = 100},
               new Activity(){Action = "run", Time = 75},
               new Activity(){Action = "jump", Time = 200}
            };
            var tasks = new List<Task>();

            //adapted from https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.run?view=net-5.0
            foreach (var action in actions)
            {
                Task t = Task.Run(() =>
               {
                   //serializes to camelCase by default
                   activityTracker.AddAction(JsonSerializer.Serialize(action));
               });
                tasks.Add(t);
            }

            Task.WaitAll(tasks.ToArray());
            var jsonStats = activityTracker.GetStats();
            var stats = JsonSerializer.Deserialize<IEnumerable<ActivityStat>>(jsonStats, jsonOptions);
            //.Single will throw exception if there is more than one
            var jumpStat = stats.Single(s => s.Action == "jump");
            var runStat = stats.Single(s => s.Action == "run");
            Assert.Equal(150, jumpStat.Avg);
            Assert.Equal(75, runStat.Avg);
        }

        [Fact]
        public void VerifyAddActionReturnsErrorWithInvalidData()
        {
            var activityTracker = new ActivityTracker();
            var nullResult = activityTracker.AddAction(null);
            var emptyResult = activityTracker.AddAction("");
            var invalidJsonResult = activityTracker.AddAction("action is jump and time is 100");
            var unexpectedJsonResult = activityTracker.AddAction(@"""action"": ""jump"", ""timeToComplete"": 100");
            var negativeTimeJsonResult = activityTracker.AddAction(JsonSerializer.Serialize(new Activity() { Action = "jump", Time = -50 }));
            Assert.NotNull(nullResult);
            Assert.NotNull(emptyResult);
            Assert.NotNull(invalidJsonResult);
            Assert.NotNull(unexpectedJsonResult);
            Assert.NotNull(negativeTimeJsonResult);
        }

        static ActivityTrackerError AddAction(ActivityTracker tracker, string action, int timeInSeconds)
        {
            return tracker.AddAction($@"{{""action"": ""{action}"", ""time"": {timeInSeconds} }}");
        }
    }
}
