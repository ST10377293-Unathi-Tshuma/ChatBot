namespace Final_Version
{
    /// <summary>
    /// Centralized constants and configuration values for the application
    /// </summary>
    public static class Constants
    {
        // Resource paths
        public const string SoundFileName = "Message.wav";
        public const string ImageFileName = "BotImage.bmp";
        public const string ResourcesFolder = "Resources";

        // Quiz configuration
        public const int QuizTotalQuestions = 10;
        public const int QuizMinScoreForExcellent = 8;
        public const int QuizMinScoreForGood = 5;
        public const int MinAnswerIndex = 1;
        public const int MaxAnswerIndex = 4;

        // ASCII Art configuration
        public const int AsciiArtWidth = 60;
        public const double AsciiArtHeightRatio = 0.55;
        public const string AsciiChars = "@#%*+=-:. ";

        // Welcome message
        public const string WelcomeMessage = 
            "Welcome to the Cybersecurity Chat Bot! Enter your name and favorite topic, then ask about topics like 'passwords', 'scams', 'privacy', or general terms like 'cybersecurity', 'firewall', or 'malware'. " +
            "You can also manage tasks with 'add task - [title]: [description]', 'view tasks', 'delete task -[index]', or 'complete task -[index]', or start a quiz with 'start quiz'.";

        // Task commands
        public const string AddTaskCommand = "add task -";
        public const string ViewTasksCommand = "view tasks";
        public const string ViewLogCommand = "view log";
        public const string DeleteTaskCommand = "delete task -";
        public const string CompleteTaskCommand = "complete task -";
        public const string StartQuizCommand = "start quiz";
        public const string RemindMeToCommand = "remind me to";

        // Follow-up keywords
        public static readonly string[] FollowUpKeywords = { "tell me more", "what else?", "more", "explain", "elaborate" };
    }
}

