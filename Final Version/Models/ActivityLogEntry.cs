using System;

namespace Final_Version.Models
{
    /// <summary>
    /// Represents an entry in the activity log
    /// </summary>
    public class ActivityLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string UserInput { get; set; }
        public string Action { get; set; }
    }
}

