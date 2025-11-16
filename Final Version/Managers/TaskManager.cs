using System;
using System.Collections.Generic;
using System.Linq;
using Final_Version.Models;

namespace Final_Version.Managers
{
    /// <summary>
    /// Manages task creation, viewing, and manipulation
    /// </summary>
    public class TaskManager
    {
        private List<TaskItem> tasks = new List<TaskItem>();
        private bool awaitingReminderResponse = false;
        private int currentTaskIndex = -1;
        private string currentTaskTitle;

        public bool IsAwaitingReminderResponse => awaitingReminderResponse;

        /// <summary>
        /// Adds a task from user input
        /// </summary>
        public TaskCreationResult AddTask(string input)
        {
            string[] parts = input.Split(new[] { "remind", "to" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                return new TaskCreationResult
                {
                    Success = false,
                    Message = "Please provide a task description after 'remind me to'. Example: 'remind me to update my password'."
                };
            }

            string description = parts[1].Trim();
            string title = description.Split(':')[0].Trim();
            currentTaskIndex = tasks.Count;
            
            tasks.Add(new TaskItem 
            { 
                Title = title, 
                Description = description, 
                Reminder = null, 
                IsCompleted = false 
            });
            
            currentTaskTitle = title;
            awaitingReminderResponse = true;

            return new TaskCreationResult
            {
                Success = true,
                Message = $"Task added with the description \"{description}\". Would you like a reminder? (e.g., 'yes in 3 days', 'yes tomorrow', 'yes 2025-06-30 12:00', or 'no')",
                TaskTitle = title
            };
        }

        /// <summary>
        /// Handles reminder response from user
        /// </summary>
        public ReminderResponseResult HandleReminderResponse(string input, string userName)
        {
            if (!awaitingReminderResponse || currentTaskIndex < 0 || currentTaskIndex >= tasks.Count)
            {
                return new ReminderResponseResult
                {
                    Success = false,
                    Message = "No task is awaiting a reminder response."
                };
            }

            string lowerInput = input.ToLower().Trim();
            
            if (lowerInput == "no")
            {
                awaitingReminderResponse = false;
                return new ReminderResponseResult
                {
                    Success = true,
                    Message = $"Got it! No reminder set for '{currentTaskTitle}', {userName}."
                };
            }
            
            if (lowerInput.StartsWith("yes"))
            {
                string timeframe = input.Substring(3).Trim();
                DateTime? reminderDate = ParseReminderTimeframe(timeframe);
                
                if (reminderDate.HasValue)
                {
                    tasks[currentTaskIndex].Reminder = reminderDate.Value;
                    awaitingReminderResponse = false;
                    return new ReminderResponseResult
                    {
                        Success = true,
                        Message = $"Got it! I'll remind you on {reminderDate.Value:yyyy-MM-dd HH:mm} for '{currentTaskTitle}', {userName}.",
                        ReminderDate = reminderDate.Value
                    };
                }
                else
                {
                    return new ReminderResponseResult
                    {
                        Success = false,
                        Message = $"Invalid timeframe, {userName}. Please use 'yes in X days', 'yes tomorrow', or 'yes 2025-06-30 12:00'."
                    };
                }
            }

            return new ReminderResponseResult
            {
                Success = false,
                Message = $"Please respond with 'yes in X days', 'yes tomorrow', 'yes 2025-06-30 12:00', or 'no', {userName}."
            };
        }

        /// <summary>
        /// Parses reminder timeframe from user input
        /// </summary>
        private DateTime? ParseReminderTimeframe(string timeframe)
        {
            if (DateTime.TryParse(timeframe, out DateTime reminderDate))
            {
                return reminderDate;
            }

            if (timeframe.Contains("in") || timeframe.Contains("tomorrow"))
            {
                int days = 0;
                if (int.TryParse(new string(timeframe.Where(char.IsDigit).ToArray()), out days) && timeframe.Contains("days"))
                {
                    return DateTime.Now.AddDays(days);
                }
                else if (timeframe.Contains("tomorrow"))
                {
                    return DateTime.Now.AddDays(1);
                }
                else
                {
                    return DateTime.Now.AddDays(1);
                }
            }

            return null;
        }

        /// <summary>
        /// Gets formatted list of all tasks
        /// </summary>
        public string GetTasksList(string userName)
        {
            if (tasks.Count == 0)
            {
                return $"{userName}, you have no tasks yet. Add one with 'add task - [title]: [description]' or 'remind me to [task]'!";
            }

            string taskList = "Your tasks:\n";
            for (int i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                string reminderText = task.Reminder.HasValue 
                    ? $"Reminder: {task.Reminder:yyyy-MM-dd HH:mm}" 
                    : "No reminder";
                string status = task.IsCompleted ? "[Completed]" : "[Pending]";
                taskList += $"{i + 1}. {task.Title} - {task.Description} {status} ({reminderText})\n";
                taskList += $"   Options: delete task -{i + 1} or complete task -{i + 1}\n";
            }
            
            return taskList;
        }

        /// <summary>
        /// Deletes a task by index
        /// </summary>
        public TaskOperationResult DeleteTask(int taskIndex, string userName)
        {
            if (taskIndex < 1 || taskIndex > tasks.Count)
            {
                return new TaskOperationResult
                {
                    Success = false,
                    Message = $"{userName}, invalid task index. Use 'view tasks' to see your task list and try again."
                };
            }

            int index = taskIndex - 1;
            string taskTitle = tasks[index].Title;
            tasks.RemoveAt(index);
            
            return new TaskOperationResult
            {
                Success = true,
                Message = $"Task {taskIndex} deleted, {userName}.",
                TaskTitle = taskTitle
            };
        }

        /// <summary>
        /// Marks a task as completed
        /// </summary>
        public TaskOperationResult CompleteTask(int taskIndex, string userName)
        {
            if (taskIndex < 1 || taskIndex > tasks.Count)
            {
                return new TaskOperationResult
                {
                    Success = false,
                    Message = $"{userName}, invalid task index. Use 'view tasks' to see your task list and try again."
                };
            }

            int index = taskIndex - 1;
            string taskTitle = tasks[index].Title;
            tasks[index].IsCompleted = true;
            
            return new TaskOperationResult
            {
                Success = true,
                Message = $"Task {taskIndex} marked as completed, {userName}.",
                TaskTitle = taskTitle
            };
        }

        /// <summary>
        /// Parses task index from input string
        /// </summary>
        public int? ParseTaskIndex(string input, string commandPrefix)
        {
            if (!input.StartsWith(commandPrefix))
            {
                return null;
            }

            string indexPart = input.Substring(commandPrefix.Length).Trim();
            if (int.TryParse(indexPart, out int taskIndex))
            {
                return taskIndex;
            }

            return null;
        }
    }

    /// <summary>
    /// Result of task creation operation
    /// </summary>
    public class TaskCreationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string TaskTitle { get; set; }
    }

    /// <summary>
    /// Result of reminder response handling
    /// </summary>
    public class ReminderResponseResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public DateTime? ReminderDate { get; set; }
    }

    /// <summary>
    /// Result of task operation (delete/complete)
    /// </summary>
    public class TaskOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string TaskTitle { get; set; }
    }
}

