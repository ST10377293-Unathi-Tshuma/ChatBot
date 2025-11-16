using System;
using System.Collections.Generic;
using System.Linq;

namespace Final_Version
{
    class Bot
    {
        // GeeksforGeeks. (n.d.). C# Delegates. 
        // [online] Available at: https://www.geeksforgeeks.org/c-sharp/c-sharp-delegates/ 
        // [Accessed 27 Jun. 2025].
        public delegate void ResponseHandler(bool recognized, string response);
        public ResponseHandler OnResponseGenerated;

        public delegate void SentimentDectectedHandler(string sentiment);
        public SentimentDectectedHandler OnSentimentDetected;

        public delegate void TopicChangedHandler(string newTopic);
        public TopicChangedHandler OnTopicChanged;

        private readonly Random random = new Random();
        private string currentTopic = null;
        private string userName;
        private string favoriteTopic;
        private bool isQuizActive = false;
        private int quizQuestionIndex = 0;
        private int quizScore = 0;
        private List<QuizQuestion> quizQuestions = new List<QuizQuestion>();
        private int selectedAnswerIndex = -1;
        private bool awaitingReminderResponse = false;
        private string currentTaskTitle;
        private List<ActivityLogEntry> activityLog = new List<ActivityLogEntry>();

        ///GeeksforGeeks. (n.d.). C# Dictionary with Examples. 
        //[online] Available at: https://www.geeksforgeeks.org/c-sharp/c-sharp-dictionary-with-examples/ 
        //[Accessed 27 Jun. 2025].
        public readonly Dictionary<string, List<string>> topicResponses = new Dictionary<string, List<string>>
        {
            {
                "password_safety",
                new List<string>
                {
                    "Always use strong, unique passwords for each account. A strong password should be at least 12 characters long, include a mix of uppercase letters, lowercase letters, numbers, and symbols, and avoid personal information like your name or birthdate. For example, instead of 'John123', use something like 'Tr0ub4dor&3xplor3r'. Consider using a password manager to generate and store complex passwords securely.",
                    "Make sure your passwords are complex and unique for every account. Aim for at least 12 characters with a combination of letters, numbers, and symbols, like 'P@ssw0rd#2025'. Avoid using easily guessable info, such as your birthday, and never reuse passwords—if one account is hacked, others could be at risk too.",
                    "Create strong passwords by using a mix of characters—uppercase, lowercase, numbers, and symbols—and make them at least 12 characters long. For instance, 'G7m!x9qL$2vP' is a good example. Don’t use the same password across multiple sites, and consider a password manager to keep track of them securely."
                }
            },
            {
                "phishing_scams",
                new List<string>
                {
                    "Be cautious of emails asking for personal information. Scammers often disguise themselves as trusted organizations. For example, you might get an email saying your account is locked and asking you to click a link—don’t! Always verify the sender’s email address and contact the organization directly if unsure.",
                    "Phishing scams trick you into sharing sensitive info, like passwords, by posing as legitimate sources. For instance, a fake email might claim you’ve won a prize and ask for your bank details. Never click links in unsolicited messages, and double-check the sender’s email for typos, like 'support@yourbannk.com'.",
                    "Scammers often use phishing to steal your data through fake emails or texts. An example is a message claiming your account needs 'urgent verification' with a link to a fake login page. Always hover over links to check the URL before clicking, and avoid sharing personal info with unknown contacts."
                }
            },
            {
                "safe_browsing_privacy",
                new List<string>
                {
                    "Protecting your privacy online starts with safe browsing habits. Always check that websites use HTTPS (look for the padlock icon in the browser), which ensures your data is encrypted. For example, 'https://www.example.com' is safer than 'http://example.com'. Be cautious about sharing personal information—avoid posting sensitive details like your address or phone number on social media.",
                    "Keep your online activity private by ensuring websites use HTTPS—check for the padlock in your browser. For instance, 'https://www.google.com' is secure, but 'http://' isn’t. Use privacy settings on social media to limit who can see your posts, and enable two-factor authentication (2FA) for extra security.",
                    "Stay safe online by browsing securely—always use HTTPS websites, which encrypt your data (e.g., 'https://www.example.com'). Don’t share personal details like your phone number publicly, and clear your browser cookies regularly to prevent tracking. Adding 2FA to your accounts also helps protect your privacy."
                }
            }
        };

        public readonly Dictionary<string, string> generalKnowledge = new Dictionary<string, string>
        {
            { "cybersecurity", "Cybersecurity is the practice of protecting computers, servers, mobile devices, electronic systems, networks, and data from digital attacks, unauthorized access, or damage. It involves a range of practices, like using strong passwords, enabling two-factor authentication, and keeping software updated. For example, a company might use firewalls and encryption to secure its data, while individuals can protect themselves by avoiding suspicious links and using antivirus software." },
            { "firewall", "A firewall is a security system that monitors and controls incoming and outgoing network traffic based on predefined rules. It acts like a barrier between your device and potential threats on the internet. For instance, a firewall might block unauthorized access to your computer while allowing safe connections, like accessing a trusted website. Firewalls are essential for both personal devices and business networks to prevent cyberattacks." },
            { "malware", "Malware, short for malicious software, is any program designed to harm or exploit a device, network, or user. Common types include viruses, worms, ransomware, and spyware. For example, ransomware can lock your files and demand payment to unlock them, while spyware might secretly track your activity. To protect yourself, always avoid downloading files from untrusted sources, keep your antivirus software updated, and be cautious with email attachments." }
        };

        public readonly Dictionary<string, string> keywordToTopic = new Dictionary<string, string>
        {
            { "password", "password_safety" }, { "passcode", "password_safety" }, { "login", "password_safety" }, { "credentials", "password_safety" }, {"passwords", "password_safety"},
            { "scam", "phishing_scams" }, { "phishing", "phishing_scams" }, { "fraud", "phishing_scams" }, { "hoax", "phishing_scams" }, { "scamming", "phishing_scams" },
            { "privacy", "safe_browsing_privacy" }, { "security", "safe_browsing_privacy" }, { "browsing", "safe_browsing_privacy" }, { "data", "safe_browsing_privacy" }, { "private", "safe_browsing_privacy" },
            { "2fa", "safe_browsing_privacy" }, { "two-factor", "safe_browsing_privacy" }
        };


        private List<TaskItem> tasks = new List<TaskItem>();

        private int currentTaskIndex;

        public void SetUserDetails(string name, string favTopic)
        {
            userName = name;
            favoriteTopic = favTopic?.ToLower().Trim() ?? "privacy";
            if (keywordToTopic.ContainsValue(GetTopicKey(favoriteTopic)))
            {
                favoriteTopic = keywordToTopic.FirstOrDefault(kvp => kvp.Value == GetTopicKey(favoriteTopic)).Key;
            }
            else
            {
                favoriteTopic = "privacy";
            }
        }

        private string GetTopicKey(string keyword)
        {
            return keywordToTopic.ContainsKey(keyword) ? keywordToTopic[keyword] : null;
        }

        private string GetTopicName(string topicKey)
        {
            if (topicKey == "password_safety") return "Password Safety";
            else if (topicKey == "phishing_scams") return "Phishing and Scams";
            else if (topicKey == "safe_browsing_privacy") return "Safe Browsing and Privacy";
            else return "this topic";
        }

        private void InitializeQuiz()
        {
            // W3Schools. (n.d.). C# Classes and Objects. 
            // [online] Available at: https://www.w3schools.com/cs/cs_classes.php [Accessed 27 Jun. 2025].
            quizQuestions = new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Question = "What is the minimum recommended length for a strong password?",
                    Options = new[] { "6 characters", "8 characters", "12 characters", "16 characters" },
                    CorrectAnswerIndex = 2,
                    Explanation = "A strong password should be at least 12 characters long to ensure better security, as recommended in password safety guidelines."
                },
                new QuizQuestion
                {
                    Question = "Which of these is a characteristic of a phishing email?",
                    Options = new[] { "It uses HTTPS", "It asks for your password via a link", "It comes from a verified sender", "It has no links" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Phishing emails often trick users into sharing sensitive information, like passwords, by including links to fake login pages."
                },
                new QuizQuestion
                {
                    Question = "What does HTTPS indicate on a website?",
                    Options = new[] { "The website is free", "The website is popular", "The website encrypts your data", "The website has no ads" },
                    CorrectAnswerIndex = 2,
                    Explanation = "HTTPS ensures that your data is encrypted, making the website safer for sharing personal information."
                },
                new QuizQuestion
                {
                    Question = "Why should you avoid reusing passwords across multiple sites?",
                    Options = new[] { "It slows down your login", "It makes passwords harder to remember", "A hack on one site risks others", "It reduces password strength" },
                    CorrectAnswerIndex = 2,
                    Explanation = "Reusing passwords means that if one account is hacked, other accounts using the same password are also at risk."
                },
                new QuizQuestion
                {
                    Question = "What should you check before clicking a link in an email?",
                    Options = new[] { "The email subject", "The sender’s email address", "The email’s length", "The email’s font" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Always verify the sender’s email address to ensure it’s legitimate and check the URL by hovering over links to avoid phishing scams."
                },
                new QuizQuestion
                {
                    Question = "What is a firewall used for?",
                    Options = new[] { "Speeding up your internet", "Blocking unauthorized network access", "Storing passwords", "Encrypting emails" },
                    CorrectAnswerIndex = 1,
                    Explanation = "A firewall monitors and controls network traffic to block unauthorized access, protecting your device from threats."
                },
                new QuizQuestion
                {
                    Question = "What is a common type of malware?",
                    Options = new[] { "Firewall", "Ransomware", "HTTPS", "Password manager" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Ransomware is a type of malware that locks your files and demands payment to unlock them."
                },
                new QuizQuestion
                {
                    Question = "What does enabling two-factor authentication (2FA) do?",
                    Options = new[] { "Speeds up login", "Adds an extra layer of security", "Changes your password", "Disables cookies" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Two-factor authentication (2FA) adds an extra layer of security by requiring a second form of verification beyond your password."
                },
                new QuizQuestion
                {
                    Question = "What should you avoid sharing on social media to protect your privacy?",
                    Options = new[] { "Your favorite color", "Your phone number", "Your hobbies", "Your pet’s name" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Avoid sharing sensitive details like your phone number on social media to protect your privacy and reduce the risk of identity theft."
                },
                new QuizQuestion
                {
                    Question = "What is a good practice to prevent tracking while browsing?",
                    Options = new[] { "Using the same browser", "Clearing browser cookies regularly", "Disabling HTTPS", "Sharing your location" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Clearing browser cookies regularly helps prevent tracking by removing data that websites use to monitor your activity."
                }
            };
        }

        private void StartQuiz()
        {
            isQuizActive = true;
            quizQuestionIndex = 0;
            quizScore = 0;
            InitializeQuiz();
            AskQuizQuestion();
            // Log the start of the quiz
            activityLog.Add(new ActivityLogEntry { Timestamp = DateTime.Now, UserInput = "start quiz", Action = "Started a quiz" });
        }

        private void AskQuizQuestion()
        {
            if (quizQuestionIndex >= quizQuestions.Count)
            {
                EndQuiz();
                return;
            }

            var question = quizQuestions[quizQuestionIndex];
            string response = $"{userName}, here’s question {quizQuestionIndex + 1} of 10:\n{question.Question}\n";
            for (int i = 0; i < question.Options.Length; i++)
            {
                response += $"{i + 1}. {question.Options[i]}\n";
            }
            response += "Please answer with the number (1-4) of your choice.";
            OnResponseGenerated?.Invoke(true, response);
        }

        private void ProcessQuizAnswer(string input)
        {
            if (!int.TryParse(input.Trim(), out int answer) || answer < 1 || answer > 4)
            {
                OnResponseGenerated?.Invoke(false, $"Please enter a number between 1 and 4, {userName}.");
                return;
            }

            selectedAnswerIndex = answer - 1;
            var question = quizQuestions[quizQuestionIndex];
            bool isCorrect = selectedAnswerIndex == question.CorrectAnswerIndex;
            if (isCorrect)
            {
                quizScore++;
            }

            string response = isCorrect
                ? $"Correct, {userName}! {question.Explanation}"
                : $"Sorry, {userName}, that’s incorrect. The correct answer was: {question.Options[question.CorrectAnswerIndex]}. {question.Explanation}";

            quizQuestionIndex++;
            response += $"\nYour current score: {quizScore}/{quizQuestionIndex}.";
            OnResponseGenerated?.Invoke(true, response);

            // Log the quiz answer
            activityLog.Add(new ActivityLogEntry { Timestamp = DateTime.Now, UserInput = input, Action = $"Answered quiz question {quizQuestionIndex} (Correct: {isCorrect})" });

            if (quizQuestionIndex < quizQuestions.Count)
            {
                AskQuizQuestion();
            }
            else
            {
                EndQuiz();
            }
        }

        private void EndQuiz()
        {
            isQuizActive = false;
            string response = $"{userName}, quiz complete! Your final score: {quizScore}/10.\n";
            if (quizScore >= 8)
            {
                response += "Excellent work! You’re a cybersecurity pro!";
            }
            else if (quizScore >= 5)
            {
                response += "Good job! Keep learning to boost your cybersecurity skills!";
            }
            else
            {
                response += "Nice try! Review topics like passwords, phishing, and privacy to improve your score next time!";
            }
            OnResponseGenerated?.Invoke(true, response);
        }

        private void AddTaskFromInput(string input)
        {
            string[] parts = input.Split(new[] { "remind", "to" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                OnResponseGenerated?.Invoke(false, $"Please provide a task description after 'remind me to', {userName}. Example: 'remind me to update my password'.");
                return;
            }

            string description = parts[1].Trim();
            string title = description.Split(':')[0].Trim();
            currentTaskIndex = tasks.Count;
            tasks.Add(new TaskItem { Title = title, Description = description, Reminder = null, IsCompleted = false });
            currentTaskTitle = title;
            awaitingReminderResponse = true;
            OnResponseGenerated?.Invoke(true, $"Task added with the description \"{description}\". Would you like a reminder? (e.g., 'yes in 3 days', 'yes tomorrow', 'yes 2025-06-30 12:00', or 'no')");

            // Log the task addition
            activityLog.Add(new ActivityLogEntry { Timestamp = DateTime.Now, UserInput = input, Action = $"Added task: {title}" });
        }

        private void HandleReminderResponse(string input)
        {
            if (awaitingReminderResponse && currentTaskIndex >= 0 && currentTaskIndex < tasks.Count)
            {
                string lowerInput = input.ToLower().Trim();
                if (lowerInput == "no")
                {
                    awaitingReminderResponse = false;
                    OnResponseGenerated?.Invoke(true, $"Got it! No reminder set for '{currentTaskTitle}', {userName}.");
                    // Log the reminder decision
                    activityLog.Add(new ActivityLogEntry { Timestamp = DateTime.Now, UserInput = input, Action = $"Set no reminder for task: {currentTaskTitle}" });
                    return;
                }
                else if (lowerInput.StartsWith("yes"))
                {
                    string timeframe = input.Substring(3).Trim();
                    DateTime reminderDate;
                    if (DateTime.TryParse(timeframe, out reminderDate))
                    {
                        tasks[currentTaskIndex].Reminder = reminderDate;
                        awaitingReminderResponse = false;
                        OnResponseGenerated?.Invoke(true, $"Got it! I'll remind you on {reminderDate:yyyy-MM-dd HH:mm} for '{currentTaskTitle}', {userName}.");
                    }
                    else if (timeframe.Contains("in") || timeframe.Contains("tomorrow"))
                    {
                        int days = 0;
                        if (int.TryParse(new string(timeframe.Where(char.IsDigit).ToArray()), out days) && timeframe.Contains("days"))
                        {
                            reminderDate = DateTime.Now.AddDays(days);
                        }
                        else if (timeframe.Contains("tomorrow"))
                        {
                            reminderDate = DateTime.Now.AddDays(1);
                        }
                        else
                        {
                            reminderDate = DateTime.Now.AddDays(1);
                        }
                        tasks[currentTaskIndex].Reminder = reminderDate;
                        awaitingReminderResponse = false;
                        OnResponseGenerated?.Invoke(true, $"Got it! I'll remind you on {reminderDate:yyyy-MM-dd HH:mm} for '{currentTaskTitle}', {userName}.");
                    }
                    else
                    {
                        OnResponseGenerated?.Invoke(false, $"Invalid timeframe, {userName}. Please use 'yes in X days', 'yes tomorrow', or 'yes 2025-06-30 12:00'.");
                        return;
                    }
                    // Log the reminder setting
                    activityLog.Add(new ActivityLogEntry { Timestamp = DateTime.Now, UserInput = input, Action = $"Set reminder for {reminderDate:yyyy-MM-dd HH:mm} on task: {currentTaskTitle}" });
                }
                else
                {
                    OnResponseGenerated?.Invoke(false, $"Please respond with 'yes in X days', 'yes tomorrow', 'yes 2025-06-30 12:00', or 'no', {userName}.");
                    return;
                }
            }
        }

        private void ViewTasks()
        {
            if (tasks.Count == 0)
            {
                OnResponseGenerated?.Invoke(true, $"{userName}, you have no tasks yet. Add one with 'add task - [title]: [description]' or 'remind me to [task]'!");
                return;
            }

            string taskList = "Your tasks:\n";
            for (int i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                string reminderText = task.Reminder.HasValue ? $"Reminder: {task.Reminder:yyyy-MM-dd HH:mm}" : "No reminder";
                string status = task.IsCompleted ? "[Completed]" : "[Pending]";
                taskList += $"{i + 1}. {task.Title} - {task.Description} {status} ({reminderText})\n";
                taskList += $"   Options: delete task -{i} or complete task -{i}\n";
            }
            OnResponseGenerated?.Invoke(true, taskList);

            // Log the task view action
            activityLog.Add(new ActivityLogEntry { Timestamp = DateTime.Now, UserInput = "view tasks", Action = "Viewed task list" });
        }

        private void ViewLog()
        {
            if (activityLog.Count == 0)
            {
                OnResponseGenerated?.Invoke(true, $"{userName}, no activities logged yet.");
                return;
            }

            string log = "Activity Log:\n";
            foreach (var entry in activityLog)
            {
                log += $"{entry.Timestamp:yyyy-MM-dd HH:mm:ss}: {entry.UserInput} - {entry.Action}\n";
            }
            OnResponseGenerated?.Invoke(true, log);

            // Log the log view action
            activityLog.Add(new ActivityLogEntry { Timestamp = DateTime.Now, UserInput = "view log", Action = "Viewed activity log" });
        }

        private void ManageTask(string input)
        {
            if (!int.TryParse(input.Split('-')[1].Trim(), out int taskIndex) || taskIndex < 1 || taskIndex > tasks.Count)
            {
                OnResponseGenerated?.Invoke(false, $"{userName}, invalid task index. Use 'view tasks' to see your task list and try again.");
                return;
            }

            int index = taskIndex - 1;
            if (input.StartsWith("delete task -"))
            {
                string taskTitle = tasks[index].Title;
                tasks.RemoveAt(index);
                OnResponseGenerated?.Invoke(true, $"Task {taskIndex} deleted, {userName}.");
                // Log the task deletion
                activityLog.Add(new ActivityLogEntry { Timestamp = DateTime.Now, UserInput = input, Action = $"Deleted task: {taskTitle}" });
            }
            else if (input.StartsWith("complete task -"))
            {
                string taskTitle = tasks[index].Title;
                tasks[index].IsCompleted = true;
                OnResponseGenerated?.Invoke(true, $"Task {taskIndex} marked as completed, {userName}.");
                // Log the task completion
                activityLog.Add(new ActivityLogEntry { Timestamp = DateTime.Now, UserInput = input, Action = $"Completed task: {taskTitle}" });
            }
        }

        private string DetectSentiment(string input)
        {
            if (input.Contains("worried") || input.Contains("scared") || input.Contains("anxious") || input.Contains("nervous"))
                return "worried";
            if (input.Contains("curious") || input.Contains("interested") || input.Contains("wondering"))
                return "curious";
            if (input.Contains("frustrated") || input.Contains("annoyed") || input.Contains("stuck"))
                return "frustrated";
            return null;
        }

        private string AdjustResponse(string baseResponse, string sentiment, bool isFollowUp)
        {
            if (string.IsNullOrEmpty(baseResponse)) return null;

            if (isFollowUp)
            {
                return $"Great, let’s dive deeper into {GetTopicName(GetTopicKey(currentTopic))}, {userName}! {baseResponse}";
            }

            if (sentiment == "worried")
            {
                return $"{userName}, it’s completely understandable to feel that way. {baseResponse} Let me share some tips to help you stay safe!";
            }
            else if (sentiment == "curious")
            {
                return $"{userName}, I’m glad you’re curious! {baseResponse} Let me know if you’d like more details!";
            }
            else if (sentiment == "frustrated")
            {
                return $"{userName}, I can see this might be frustrating. {baseResponse} Don’t worry, I’m here to simplify it for you—take it step by step!";
            }
            else
            {
                return $"{userName}, here’s some info: {baseResponse}";
            }
        }

        //Matlock, T., 2023.
        //Exploring Natural Language Processing with C#: From Basics to Advanced Techniques.
        //[online] Medium. Available at: https://medium.com/@thomasmatlockbba/exploring-natural-language-processing-with-c-from-basics-to-advanced-techniques-9114f7da802a
        //[Accessed 26 Jun. 2025].
        public void ProcessInput(string input)
        {
            string lowerInput = input.ToLower().Trim();
            string baseResponse = "";

            if (isQuizActive)
            {
                ProcessQuizAnswer(input);
                return;
            }

            // NLP: Check for "start quiz" keyword to initiate quiz mode
            if (lowerInput == "start quiz")
            {
                StartQuiz();
                return;
            }
            // NLP: Check for "add task -" keyword to process structured task input
            else if (lowerInput.StartsWith("add task -"))
            {
                AddTaskFromInput("add task -" + lowerInput.Substring(9));
                return;
            }
            // NLP: Check for "view tasks" keyword to display task list
            else if (lowerInput == "view tasks")
            {
                ViewTasks();
                return;
            }
            // NLP: Check for "view log" keyword to display activity log
            else if (lowerInput == "view log")
            {
                ViewLog();
                return;
            }
            // NLP: Check for "delete task -" or "complete task -" keywords to manage tasks
            else if (lowerInput.StartsWith("delete task -") || lowerInput.StartsWith("complete task -"))
            {
                ManageTask(lowerInput);
                return;
            }
            else if (awaitingReminderResponse)
            {
                HandleReminderResponse(input);
                return;
            }
            // NLP: Check for "remind me to" phrase to detect and create a task with reminder intent
            else if (lowerInput.Contains("remind me to"))
            {
                AddTaskFromInput(lowerInput);
                return;
            }

            bool isFollowUp = currentTopic != null && (lowerInput == "tell me more" || lowerInput == "what else?" ||
                lowerInput.Contains("more") || lowerInput.Contains("explain") || lowerInput.Contains("elaborate"));

            // NLP: Check for follow-up intent using Contains for keywords like "more", "explain", or "elaborate"
            if (isFollowUp)
            {
                if (topicResponses.ContainsKey(GetTopicKey(currentTopic)))
                {
                    baseResponse = topicResponses[GetTopicKey(currentTopic)][random.Next(topicResponses[GetTopicKey(currentTopic)].Count)];
                    // Log the follow-up action
                    activityLog.Add(new ActivityLogEntry { Timestamp = DateTime.Now, UserInput = input, Action = $"Requested more info on {GetTopicName(GetTopicKey(currentTopic))}" });
                }
                else
                {
                    OnResponseGenerated?.Invoke(false, "It seems we lost track of the topic. Please ask about a new topic like 'passwords', 'scams', or 'privacy'.");
                    return;
                }
            }
            // NLP: Check for general knowledge topics using Contains on split key strings
            else if (generalKnowledge.Any(gk => gk.Key.Split(',').Any(k => lowerInput.Contains(k.Trim()))))
            {
                var key = generalKnowledge.Keys.First(gk => gk.Split(',').Any(k => lowerInput.Contains(k.Trim())));
                baseResponse = generalKnowledge[key];
                currentTopic = null;
                // Log the general knowledge request
                activityLog.Add(new ActivityLogEntry { Timestamp = DateTime.Now, UserInput = input, Action = $"Requested info on {key}" });
            }
            // NLP: Check for topic-related keywords using Contains on keywordToTopic keys
            else if (keywordToTopic.Any(kvp => lowerInput.Contains(kvp.Key)))
            {
                currentTopic = keywordToTopic.First(kvp => lowerInput.Contains(kvp.Key)).Key;
                baseResponse = topicResponses[GetTopicKey(currentTopic)][random.Next(topicResponses[GetTopicKey(currentTopic)].Count)];
                // Log the topic request
                activityLog.Add(new ActivityLogEntry { Timestamp = DateTime.Now, UserInput = input, Action = $"Requested info on {GetTopicName(GetTopicKey(currentTopic))}" });
            }
            else if (lowerInput.Contains("favorite topic") || lowerInput.Contains("interested in"))
            {
                string newFavTopic = lowerInput.Split(new[] { "favorite topic", "interested in" }, StringSplitOptions.None)
                    .Last().Trim();
                // NLP: Use Split to extract the new favorite topic from the input
                if (!string.IsNullOrEmpty(newFavTopic) && keywordToTopic.ContainsKey(newFavTopic))
                {
                    favoriteTopic = newFavTopic;
                    OnResponseGenerated?.Invoke(true, $"Great, {userName}! I’ll remember that your favorite topic is {GetTopicName(GetTopicKey(favoriteTopic))}. It’s a key area for staying safe online!");
                    // Log the favorite topic change
                    activityLog.Add(new ActivityLogEntry { Timestamp = DateTime.Now, UserInput = input, Action = $"Set favorite topic to {GetTopicName(GetTopicKey(favoriteTopic))}" });
                    return;
                }
                OnResponseGenerated?.Invoke(true, $"{userName}, I couldn’t identify a valid topic. Please specify a topic like 'passwords', 'scams', or 'privacy' as your favorite.");
                return;
            }
            else if (currentTopic == null && favoriteTopic != null)
            {
                baseResponse = topicResponses[GetTopicKey(favoriteTopic)][random.Next(topicResponses[GetTopicKey(favoriteTopic)].Count)];
            }

            string sentiment = isFollowUp ? null : DetectSentiment(lowerInput);
            if (sentiment != null) OnSentimentDetected?.Invoke(sentiment);

            string finalResponse = AdjustResponse(baseResponse, sentiment, isFollowUp);
            if (!string.IsNullOrEmpty(finalResponse))
            {
                OnResponseGenerated?.Invoke(true, finalResponse);
                // Log the response generation
                activityLog.Add(new ActivityLogEntry { Timestamp = DateTime.Now, UserInput = input, Action = "Generated response" });
                return;
            }

            if (!lowerInput.Contains("more") && !lowerInput.Contains("explain") && !lowerInput.Contains("elaborate"))
            {
                if (currentTopic != null)
                {
                    currentTopic = null;
                    OnTopicChanged?.Invoke(null);
                }
            }

            OnResponseGenerated?.Invoke(false, $"I’m not sure I understand, {userName}. Can you try asking about '{string.Join(", ", keywordToTopic.Keys)}', general topics like 'cybersecurity', 'firewall', or 'malware', say 'tell me more' to expand on the last topic, tell me your 'favorite topic', start a quiz with 'start quiz', add a task with 'add task - [title]: [description]' or 'remind me to [task]', view tasks with 'view tasks', view log with 'view log', or rephrase your question? Let me know how you feel!");
        }
    }
}