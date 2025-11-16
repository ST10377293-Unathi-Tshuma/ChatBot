using System;
using System.Linq;
using Final_Version.Managers;
using Final_Version.Services;
using Final_Version.Models;

namespace Final_Version.Core
{
    /// <summary>
    /// Main bot class that coordinates all bot functionality
    /// </summary>
    public class Bot
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

        private readonly QuizManager quizManager;
        private readonly TaskManager taskManager;
        private readonly ResponseGenerator responseGenerator;
        private readonly ActivityLogger activityLogger;
        private readonly ConfigurationManager configManager;
        private readonly EnhancedSentimentAnalyzer sentimentAnalyzer;
        private readonly NLPService nlpService;
        private readonly ConversationContext conversationContext;
        
        private string userName;
        private string favoriteTopic;

        public Bot()
        {
            configManager = new ConfigurationManager();
            quizManager = new QuizManager();
            taskManager = new TaskManager();
            responseGenerator = new ResponseGenerator();
            activityLogger = new ActivityLogger();
            nlpService = new NLPService();
            conversationContext = new ConversationContext();
            
            // Initialize sentiment analyzer with config keywords
            var sentimentKeywords = configManager.Configuration.SentimentKeywords;
            sentimentAnalyzer = new EnhancedSentimentAnalyzer(sentimentKeywords.Count > 0 
                ? sentimentKeywords 
                : null);
        }

        /// <summary>
        /// Sets user details including name and favorite topic
        /// </summary>
        public void SetUserDetails(string name, string favTopic)
        {
            userName = name ?? configManager.GetDefaultUserName();
            favoriteTopic = favTopic?.ToLower().Trim() ?? configManager.GetDefaultFavoriteTopic();
            
            string topicKey = responseGenerator.GetTopicKey(favoriteTopic);
            if (topicKey != null && responseGenerator.KeywordToTopic.ContainsValue(topicKey))
            {
                favoriteTopic = responseGenerator.KeywordToTopic.FirstOrDefault(kvp => kvp.Value == topicKey).Key;
            }
            else
            {
                favoriteTopic = configManager.GetDefaultFavoriteTopic();
            }
        }

        /// <summary>
        /// Processes user input and generates appropriate responses using NLP
        /// </summary>
        public void ProcessInput(string input)
        {
            string lowerInput = input.ToLower().Trim();
            string baseResponse = "";

            // Perform NLP analysis
            var nlpResult = nlpService.Analyze(input);

            // Handle quiz mode
            if (quizManager.IsQuizActive)
            {
                ProcessQuizAnswer(input);
                conversationContext.AddTurn(input, "", nlpResult.Intent);
                return;
            }

            // Handle task reminder responses
            if (taskManager.IsAwaitingReminderResponse)
            {
                HandleReminderResponse(input);
                conversationContext.AddTurn(input, "", nlpResult.Intent);
                return;
            }

            // Handle greetings first
            if (nlpResult.Intent == "greeting" || lowerInput == "hello" || lowerInput == "hi" || 
                lowerInput == "hey" || lowerInput.StartsWith("good morning") || 
                lowerInput.StartsWith("good afternoon") || lowerInput.StartsWith("good evening"))
            {
                HandleGreeting(nlpResult);
                conversationContext.AddTurn(input, "", nlpResult.Intent);
                return;
            }

            // Handle commands using NLP intent recognition
            if (TryHandleCommandWithNLP(nlpResult, input, lowerInput))
            {
                return;
            }

            // Handle follow-up requests using NLP
            bool isFollowUp = nlpResult.Intent == "follow_up" || 
                             responseGenerator.IsFollowUpRequest(lowerInput, configManager.GetFollowUpKeywords());
            if (isFollowUp)
            {
                HandleFollowUpRequest(lowerInput, input, nlpResult);
                return;
            }

            // Handle general knowledge queries with semantic matching
            baseResponse = responseGenerator.GetGeneralKnowledgeResponse(lowerInput);
            if (!string.IsNullOrEmpty(baseResponse))
            {
                activityLogger.Log(input, $"Requested info on general knowledge topic");
                GenerateFinalResponse(baseResponse, null, false, lowerInput, nlpResult);
                conversationContext.AddTurn(input, baseResponse, nlpResult.Intent);
                return;
            }

            // Handle topic-based queries with semantic matching
            baseResponse = responseGenerator.GetResponseFromKeywords(lowerInput, nlpService);
            if (!string.IsNullOrEmpty(baseResponse))
            {
                string topicName = responseGenerator.GetTopicName(responseGenerator.GetTopicKey(responseGenerator.CurrentTopic));
                activityLogger.Log(input, $"Requested info on {topicName}");
                conversationContext.CurrentTopic = topicName;
                GenerateFinalResponse(baseResponse, null, false, lowerInput, nlpResult);
                conversationContext.AddTurn(input, baseResponse, nlpResult.Intent);
                return;
            }

            // Handle favorite topic queries
            if (TryHandleFavoriteTopicQuery(lowerInput, input, nlpResult))
            {
                return;
            }

            // Default to favorite topic if no specific query
            if (responseGenerator.CurrentTopic == null && !string.IsNullOrEmpty(favoriteTopic))
            {
                string topicKey = responseGenerator.GetTopicKey(favoriteTopic);
                baseResponse = responseGenerator.GetTopicResponse(topicKey, nlpService, input);
                GenerateFinalResponse(baseResponse, null, false, lowerInput, nlpResult);
                conversationContext.AddTurn(input, baseResponse, nlpResult.Intent);
                return;
            }

            // Generate fallback response with NLP context
            GenerateFallbackResponse(nlpResult);
        }

        /// <summary>
        /// Handles quiz answer processing
        /// </summary>
        private void ProcessQuizAnswer(string input)
        {
            var result = quizManager.ProcessAnswer(input, userName);
            
            if (!result.IsValid)
            {
                OnResponseGenerated?.Invoke(false, result.Message);
                return;
            }

            activityLogger.Log(input, $"Answered quiz question (Correct: {result.IsCorrect})");
            OnResponseGenerated?.Invoke(true, result.Message);

            if (result.IsComplete)
            {
                string finalMessage = quizManager.GetFinalScoreMessage(userName);
                OnResponseGenerated?.Invoke(true, finalMessage);
                activityLogger.Log("quiz complete", "Completed quiz");
            }
            else
            {
                string nextQuestion = quizManager.GetCurrentQuestion(userName);
                if (nextQuestion != null)
                {
                    OnResponseGenerated?.Invoke(true, nextQuestion);
                }
            }
        }

        /// <summary>
        /// Handles reminder response from user
        /// </summary>
        private void HandleReminderResponse(string input)
        {
            var result = taskManager.HandleReminderResponse(input, userName);
            
            if (result.Success)
            {
                activityLogger.Log(input, result.ReminderDate.HasValue 
                    ? $"Set reminder for {result.ReminderDate.Value:yyyy-MM-dd HH:mm}" 
                    : "Set no reminder");
            }
            
            OnResponseGenerated?.Invoke(result.Success, result.Message);
        }

        /// <summary>
        /// Tries to handle command-based input using NLP and configuration
        /// </summary>
        private bool TryHandleCommandWithNLP(NLPResult nlpResult, string originalInput, string lowerInput)
        {
            // Use NLP intent if it's a command
            if (nlpResult.Intent == "start_quiz" || configManager.MatchesCommand(lowerInput, "startQuiz"))
            {
                quizManager.StartQuiz();
                string question = quizManager.GetCurrentQuestion(userName);
                OnResponseGenerated?.Invoke(true, question);
                activityLogger.Log(originalInput, "Started a quiz");
                conversationContext.AddTurn(originalInput, question, nlpResult.Intent);
                return true;
            }

            if (nlpResult.Intent == "add_task" || nlpResult.Intent == "remind_me")
            {
                var addTaskAliases = configManager.GetCommandAliases("addTask");
                bool isAddTask = addTaskAliases.Any(alias => lowerInput.StartsWith(alias));
                
                if (isAddTask || nlpResult.Intent == "add_task")
                {
                    var result = taskManager.AddTask(originalInput);
                    OnResponseGenerated?.Invoke(result.Success, result.Message);
                    if (result.Success)
                    {
                        activityLogger.Log(originalInput, $"Added task: {result.TaskTitle}");
                    }
                    conversationContext.AddTurn(originalInput, result.Message, nlpResult.Intent);
                    return true;
                }
            }

            if (nlpResult.Intent == "view_tasks" || configManager.MatchesCommand(lowerInput, "viewTasks"))
            {
                string taskList = taskManager.GetTasksList(userName);
                OnResponseGenerated?.Invoke(true, taskList);
                activityLogger.Log(originalInput, "Viewed task list");
                conversationContext.AddTurn(originalInput, taskList, nlpResult.Intent);
                return true;
            }

            if (nlpResult.Intent == "view_log" || configManager.MatchesCommand(lowerInput, "viewLog"))
            {
                string log = activityLogger.GetLog(userName);
                OnResponseGenerated?.Invoke(true, log);
                activityLogger.Log(originalInput, "Viewed activity log");
                conversationContext.AddTurn(originalInput, log, nlpResult.Intent);
                return true;
            }

            // Handle delete and complete tasks with NLP
            if (nlpResult.Intent == "delete_task" || nlpResult.Intent == "complete_task")
            {
                var deleteTaskAliases = configManager.GetCommandAliases("deleteTask");
                var completeTaskAliases = configManager.GetCommandAliases("completeTask");
                
                foreach (var alias in deleteTaskAliases)
                {
                    if (lowerInput.StartsWith(alias))
                    {
                        int? taskIndex = taskManager.ParseTaskIndex(lowerInput, alias);
                        if (taskIndex.HasValue)
                        {
                            var result = taskManager.DeleteTask(taskIndex.Value, userName);
                            OnResponseGenerated?.Invoke(result.Success, result.Message);
                            if (result.Success)
                            {
                                activityLogger.Log(originalInput, $"Deleted task: {result.TaskTitle}");
                            }
                            conversationContext.AddTurn(originalInput, result.Message, nlpResult.Intent);
                        }
                        else
                        {
                            OnResponseGenerated?.Invoke(false, $"{userName}, invalid task index. Use 'view tasks' to see your task list and try again.");
                            conversationContext.AddTurn(originalInput, "", nlpResult.Intent);
                        }
                        return true;
                    }
                }

                foreach (var alias in completeTaskAliases)
                {
                    if (lowerInput.StartsWith(alias))
                    {
                        int? taskIndex = taskManager.ParseTaskIndex(lowerInput, alias);
                        if (taskIndex.HasValue)
                        {
                            var result = taskManager.CompleteTask(taskIndex.Value, userName);
                            OnResponseGenerated?.Invoke(result.Success, result.Message);
                            if (result.Success)
                            {
                                activityLogger.Log(originalInput, $"Completed task: {result.TaskTitle}");
                            }
                            conversationContext.AddTurn(originalInput, result.Message, nlpResult.Intent);
                        }
                        else
                        {
                            OnResponseGenerated?.Invoke(false, $"{userName}, invalid task index. Use 'view tasks' to see your task list and try again.");
                            conversationContext.AddTurn(originalInput, "", nlpResult.Intent);
                        }
                        return true;
                    }
                }
            }

            // Fallback to original command handling
            return TryHandleCommand(lowerInput, originalInput);
        }

        /// <summary>
        /// Tries to handle command-based input using configuration (fallback)
        /// </summary>
        private bool TryHandleCommand(string lowerInput, string originalInput)
        {
            // Check start quiz command
            if (configManager.MatchesCommand(lowerInput, "startQuiz"))
            {
                quizManager.StartQuiz();
                string question = quizManager.GetCurrentQuestion(userName);
                OnResponseGenerated?.Invoke(true, question);
                activityLogger.Log(originalInput, "Started a quiz");
                return true;
            }

            // Check add task command
            var addTaskAliases = configManager.GetCommandAliases("addTask");
            foreach (var alias in addTaskAliases)
            {
                if (lowerInput.StartsWith(alias))
                {
                    string taskInput = alias + lowerInput.Substring(alias.Length);
                    var result = taskManager.AddTask(taskInput);
                    OnResponseGenerated?.Invoke(result.Success, result.Message);
                    if (result.Success)
                    {
                        activityLogger.Log(originalInput, $"Added task: {result.TaskTitle}");
                    }
                    return true;
                }
            }

            // Check view tasks command
            if (configManager.MatchesCommand(lowerInput, "viewTasks"))
            {
                string taskList = taskManager.GetTasksList(userName);
                OnResponseGenerated?.Invoke(true, taskList);
                activityLogger.Log(originalInput, "Viewed task list");
                return true;
            }

            // Check view log command
            if (configManager.MatchesCommand(lowerInput, "viewLog"))
            {
                string log = activityLogger.GetLog(userName);
                OnResponseGenerated?.Invoke(true, log);
                activityLogger.Log(originalInput, "Viewed activity log");
                return true;
            }

            // Check delete task command
            var deleteTaskAliases = configManager.GetCommandAliases("deleteTask");
            foreach (var alias in deleteTaskAliases)
            {
                if (lowerInput.StartsWith(alias))
                {
                    int? taskIndex = taskManager.ParseTaskIndex(lowerInput, alias);
                    if (taskIndex.HasValue)
                    {
                        var result = taskManager.DeleteTask(taskIndex.Value, userName);
                        OnResponseGenerated?.Invoke(result.Success, result.Message);
                        if (result.Success)
                        {
                            activityLogger.Log(originalInput, $"Deleted task: {result.TaskTitle}");
                        }
                    }
                    else
                    {
                        OnResponseGenerated?.Invoke(false, $"{userName}, invalid task index. Use 'view tasks' to see your task list and try again.");
                    }
                    return true;
                }
            }

            // Check complete task command
            var completeTaskAliases = configManager.GetCommandAliases("completeTask");
            foreach (var alias in completeTaskAliases)
            {
                if (lowerInput.StartsWith(alias))
                {
                    int? taskIndex = taskManager.ParseTaskIndex(lowerInput, alias);
                    if (taskIndex.HasValue)
                    {
                        var result = taskManager.CompleteTask(taskIndex.Value, userName);
                        OnResponseGenerated?.Invoke(result.Success, result.Message);
                        if (result.Success)
                        {
                            activityLogger.Log(originalInput, $"Completed task: {result.TaskTitle}");
                        }
                    }
                    else
                    {
                        OnResponseGenerated?.Invoke(false, $"{userName}, invalid task index. Use 'view tasks' to see your task list and try again.");
                    }
                    return true;
                }
            }

            // Check remind me to command
            var remindMeAliases = configManager.GetCommandAliases("remindMeTo");
            foreach (var alias in remindMeAliases)
            {
                if (lowerInput.Contains(alias))
                {
                    var result = taskManager.AddTask(lowerInput);
                    OnResponseGenerated?.Invoke(result.Success, result.Message);
                    if (result.Success)
                    {
                        activityLogger.Log(originalInput, $"Added task: {result.TaskTitle}");
                    }
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Handles greeting messages
        /// </summary>
        private void HandleGreeting(NLPResult nlpResult)
        {
            var greetings = new[]
            {
                $"Hello, {userName}! 👋 I'm your cybersecurity assistant. I'm here to help you learn about online safety, passwords, phishing, privacy, and more. What would you like to know?",
                $"Hi there, {userName}! 🛡️ Welcome! I can help you with cybersecurity topics like password safety, avoiding scams, protecting your privacy, and general security questions. What interests you?",
                $"Hey {userName}! 😊 Great to meet you! I'm ready to help with any cybersecurity questions you have. You can ask about topics like 'passwords', 'phishing', 'privacy', or start a quiz with 'start quiz'. What would you like to explore?",
                $"Greetings, {userName}! 🚀 I'm excited to help you learn about cybersecurity. Feel free to ask me anything about staying safe online, or try one of these: ask about 'passwords', learn about 'scams', explore 'privacy', or say 'start quiz' to test your knowledge!"
            };

            var random = new Random();
            string greeting = greetings[random.Next(greetings.Length)];
            OnResponseGenerated?.Invoke(true, greeting);
            activityLogger.Log("", "Greeting received");
        }

        /// <summary>
        /// Handles follow-up requests for more information with NLP
        /// </summary>
        private void HandleFollowUpRequest(string lowerInput, string originalInput, NLPResult nlpResult)
        {
            string topicKey = responseGenerator.GetTopicKey(responseGenerator.CurrentTopic);
            if (topicKey != null)
            {
                string baseResponse = responseGenerator.GetTopicResponse(topicKey, nlpService, originalInput);
                string topicName = responseGenerator.GetTopicName(topicKey);
                activityLogger.Log(originalInput, $"Requested more info on {topicName}");
                GenerateFinalResponse(baseResponse, null, true, lowerInput, nlpResult);
                conversationContext.AddTurn(originalInput, baseResponse, nlpResult.Intent);
            }
            else
            {
                string message = "It seems we lost track of the topic. Please ask about a new topic like 'passwords', 'scams', or 'privacy'.";
                OnResponseGenerated?.Invoke(false, message);
                conversationContext.AddTurn(originalInput, message, nlpResult.Intent);
            }
        }

        /// <summary>
        /// Tries to handle favorite topic queries with NLP
        /// </summary>
        private bool TryHandleFavoriteTopicQuery(string lowerInput, string originalInput, NLPResult nlpResult)
        {
            if (lowerInput.Contains("favorite topic") || lowerInput.Contains("interested in"))
            {
                string[] separators = { "favorite topic", "interested in" };
                string newFavTopic = lowerInput.Split(separators, StringSplitOptions.None)
                    .Last().Trim();

                if (!string.IsNullOrEmpty(newFavTopic) && responseGenerator.KeywordToTopic.ContainsKey(newFavTopic))
                {
                    favoriteTopic = newFavTopic;
                    string topicName = responseGenerator.GetTopicName(responseGenerator.GetTopicKey(favoriteTopic));
                    OnResponseGenerated?.Invoke(true, $"Great, {userName}! I'll remember that your favorite topic is {topicName}. It's a key area for staying safe online!");
                    activityLogger.Log(originalInput, $"Set favorite topic to {topicName}");
                    return true;
                }
                
                OnResponseGenerated?.Invoke(true, $"{userName}, I couldn't identify a valid topic. Please specify a topic like 'passwords', 'scams', or 'privacy' as your favorite.");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Generates final response with sentiment adjustment and NLP context
        /// </summary>
        private void GenerateFinalResponse(string baseResponse, string sentiment, bool isFollowUp, 
            string lowerInput, NLPResult nlpResult = null)
        {
            if (string.IsNullOrEmpty(baseResponse))
            {
                return;
            }

            // Use enhanced sentiment analysis if enabled and sentiment not already detected
            if (configManager.IsSentimentAnalysisEnabled() && sentiment == null && !isFollowUp)
            {
                // Prefer NLP sentiment indicators if available
                if (nlpResult != null && nlpResult.SentimentIndicators.Count > 0)
                {
                    sentiment = nlpResult.SentimentIndicators.First();
                }
                else
                {
                    var sentimentResult = sentimentAnalyzer.Analyze(lowerInput);
                    if (sentimentResult.Sentiment != "neutral" && sentimentResult.Confidence > 0.3)
                    {
                        sentiment = sentimentResult.Sentiment;
                        OnSentimentDetected?.Invoke(sentiment);
                    }
                }
            }

            string finalResponse = responseGenerator.AdjustResponse(baseResponse, sentiment, isFollowUp, 
                userName, nlpResult, conversationContext);
            
            if (!string.IsNullOrEmpty(finalResponse))
            {
                OnResponseGenerated?.Invoke(true, finalResponse);
                activityLogger.Log("", "Generated response");
            }
        }

        /// <summary>
        /// Generates fallback response when input is not recognized, using NLP context
        /// </summary>
        private void GenerateFallbackResponse(NLPResult nlpResult = null)
        {
            string keywords = string.Join(", ", responseGenerator.KeywordToTopic.Keys);
            
            // Use NLP to provide more helpful suggestions
            string message = $"I'm not sure I understand, {userName}. ";
            
            if (nlpResult != null && nlpResult.Keywords.Count > 0)
            {
                message += $"I noticed you mentioned '{string.Join("', '", nlpResult.Keywords.Take(3))}'. ";
            }
            
            message += "Can you try asking about '" + keywords + "', " +
                "general topics like 'cybersecurity', 'firewall', or 'malware', say 'tell me more' to expand on the last topic, " +
                "tell me your 'favorite topic', start a quiz with 'start quiz', add a task with 'add task - [title]: [description]' " +
                "or 'remind me to [task]', view tasks with 'view tasks', view log with 'view log', or rephrase your question? Let me know how you feel!";
            
            OnResponseGenerated?.Invoke(false, message);
            if (nlpResult != null)
            {
                conversationContext.AddTurn("", message, nlpResult.Intent);
            }
        }
    }
}

