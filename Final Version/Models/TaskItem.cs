using System;

namespace Final_Version.Models
{
    /// <summary>
    /// Represents a task item with title, description, reminder, and completion status
    /// </summary>
    public class TaskItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? Reminder { get; set; }
        public bool IsCompleted { get; set; }
    }
}

