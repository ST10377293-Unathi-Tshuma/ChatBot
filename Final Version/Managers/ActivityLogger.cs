using System;
using System.Collections.Generic;
using Final_Version.Models;

namespace Final_Version.Managers
{
    /// <summary>
    /// Manages activity logging for user interactions
    /// </summary>
    public class ActivityLogger
    {
        private List<ActivityLogEntry> activityLog = new List<ActivityLogEntry>();

        /// <summary>
        /// Logs an activity entry
        /// </summary>
        public void Log(string userInput, string action)
        {
            activityLog.Add(new ActivityLogEntry
            {
                Timestamp = DateTime.Now,
                UserInput = userInput,
                Action = action
            });
        }

        /// <summary>
        /// Gets formatted activity log
        /// </summary>
        public string GetLog(string userName)
        {
            if (activityLog.Count == 0)
            {
                return $"{userName}, no activities logged yet.";
            }

            string log = "Activity Log:\n";
            foreach (var entry in activityLog)
            {
                log += $"{entry.Timestamp:yyyy-MM-dd HH:mm:ss}: {entry.UserInput} - {entry.Action}\n";
            }
            
            return log;
        }

        /// <summary>
        /// Clears the activity log
        /// </summary>
        public void Clear()
        {
            activityLog.Clear();
        }
    }
}

