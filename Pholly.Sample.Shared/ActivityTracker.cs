using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pholly.Sample.Shared
{
    public class ActivityTracker
    {
        //initialize to default concurrency and capacity bc we don't know where this is going to run or how many actions to record
        private readonly ConcurrentDictionary<string, ActivityStat> _activityStats = new();
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };
        
        /// <summary>
        /// Record an activity
        /// </summary>
        /// <param name="jsonAction">json representation of an action and time in seconds. e.g. {"activity": "jump", "time": 90}</param>
        /// <returns></returns>
        public ActivityTrackerError AddAction(string jsonAction)
        {
            Activity activity;

            try
            {
                activity = JsonSerializer.Deserialize<Activity>(jsonAction, _jsonOptions);
            }
            catch (Exception ex)
            {
                if (ex is ArgumentNullException || ex is JsonException || ex is NotSupportedException)
                {
                    //In C#, we generally do not return errors because only one return value is allowed per function. Exception handling is done through throw/catch.
                    //It's good practice to let the exception bubble up to the application instead of handling it.
                    //But here we return an ActivityTrackerError to model that of golang, and to satisfy the requirements.
                    return new ActivityTrackerError("AddAction error: jsonAction is null, empty, or not in a valid format");
                }
                //if an exception is thrown we weren't expecting, keep propagating it
                throw;
            }

            //atomic and thread-safe to handle concurrent requests
            _activityStats.AddOrUpdate(activity.Action.ToLower(),
                new ActivityStat()
                {
                    Action = activity.Action.ToLower(),
                    Count = 1,
                    TotalTimeInSeconds = activity.Time
                },
                (key, stat) =>
                {
                    stat.TotalTimeInSeconds += activity.Time;
                    stat.Count++;
                    return stat;
                });

            return null;
        }

        /// <summary>
        /// Get average time per activity on all activities recorded through AddAction
        /// </summary>
        /// <returns>json array prettified representation of activity average times. e.g. [{"action": "jump", "avg": 150}]</returns>
        public string GetStats()
        {
            var avgs = _activityStats.Select(stat => new { action = stat.Key, avg = stat.Value.AvgTimeInSeconds });
            return JsonSerializer.Serialize(avgs, _jsonOptions);
        }
    }
}
