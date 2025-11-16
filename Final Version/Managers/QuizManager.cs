using System;
using System.Collections.Generic;
using System.Linq;
using Final_Version.Models;
using Final_Version.Services;
using static Final_Version.Constants;

namespace Final_Version.Managers
{
    /// <summary>
    /// Manages quiz functionality including questions, answers, and scoring
    /// </summary>
    public class QuizManager
    {
        private readonly Random random = new Random();
        private readonly DataLoader dataLoader;
        private bool isQuizActive = false;
        private int quizQuestionIndex = 0;
        private int quizScore = 0;
        private List<QuizQuestion> quizQuestions = new List<QuizQuestion>();

        public bool IsQuizActive => isQuizActive;

        public QuizManager()
        {
            dataLoader = new DataLoader();
        }

        /// <summary>
        /// Initializes the quiz with questions from JSON file or fallback
        /// </summary>
        private void InitializeQuiz()
        {
            quizQuestions = dataLoader.LoadQuizQuestions();

            // Fallback to hardcoded questions if loading failed
            if (quizQuestions.Count == 0)
            {
                LoadFallbackQuestions();
            }
        }

        /// <summary>
        /// Fallback to hardcoded questions if JSON loading fails
        /// </summary>
        private void LoadFallbackQuestions()
        {
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
                    Options = new[] { "The email subject", "The sender's email address", "The email's length", "The email's font" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Always verify the sender's email address to ensure it's legitimate and check the URL by hovering over links to avoid phishing scams."
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
                    Options = new[] { "Your favorite color", "Your phone number", "Your hobbies", "Your pet's name" },
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

        /// <summary>
        /// Starts a new quiz session
        /// </summary>
        public void StartQuiz()
        {
            isQuizActive = true;
            quizQuestionIndex = 0;
            quizScore = 0;
            InitializeQuiz();
        }

        /// <summary>
        /// Gets the current quiz question as a formatted string
        /// </summary>
        public string GetCurrentQuestion(string userName)
        {
            if (quizQuestionIndex >= quizQuestions.Count)
            {
                return null;
            }

            var question = quizQuestions[quizQuestionIndex];
            string response = $"{userName}, here's question {quizQuestionIndex + 1} of {quizQuestions.Count}:\n{question.Question}\n";
            
            for (int i = 0; i < question.Options.Length; i++)
            {
                response += $"{i + 1}. {question.Options[i]}\n";
            }
            
            response += "Please answer with the number (1-4) of your choice.";
            return response;
        }

        /// <summary>
        /// Processes a quiz answer and returns the result
        /// </summary>
        public QuizAnswerResult ProcessAnswer(string input, string userName)
        {
            if (!int.TryParse(input.Trim(), out int answer) || 
                answer < MinAnswerIndex || 
                answer > MaxAnswerIndex)
            {
                return new QuizAnswerResult
                {
                    IsValid = false,
                    Message = $"Please enter a number between {MinAnswerIndex} and {MaxAnswerIndex}, {userName}."
                };
            }

            int selectedAnswerIndex = answer - 1;
            var question = quizQuestions[quizQuestionIndex];
            bool isCorrect = selectedAnswerIndex == question.CorrectAnswerIndex;
            
            if (isCorrect)
            {
                quizScore++;
            }

            string response = isCorrect
                ? $"Correct, {userName}! {question.Explanation}"
                : $"Sorry, {userName}, that's incorrect. The correct answer was: {question.Options[question.CorrectAnswerIndex]}. {question.Explanation}";

            quizQuestionIndex++;
            response += $"\nYour current score: {quizScore}/{quizQuestionIndex}.";

            bool isComplete = quizQuestionIndex >= quizQuestions.Count;

            return new QuizAnswerResult
            {
                IsValid = true,
                IsCorrect = isCorrect,
                Message = response,
                IsComplete = isComplete,
                Score = quizScore,
                TotalQuestions = quizQuestions.Count
            };
        }

        /// <summary>
        /// Ends the quiz and returns the final score message
        /// </summary>
        public string GetFinalScoreMessage(string userName)
        {
            isQuizActive = false;
            string response = $"{userName}, quiz complete! Your final score: {quizScore}/{quizQuestions.Count}.\n";
            
            if (quizScore >= QuizMinScoreForExcellent)
            {
                response += "Excellent work! You're a cybersecurity pro!";
            }
            else if (quizScore >= QuizMinScoreForGood)
            {
                response += "Good job! Keep learning to boost your cybersecurity skills!";
            }
            else
            {
                response += "Nice try! Review topics like passwords, phishing, and privacy to improve your score next time!";
            }
            
            return response;
        }
    }

    /// <summary>
    /// Result of processing a quiz answer
    /// </summary>
    public class QuizAnswerResult
    {
        public bool IsValid { get; set; }
        public bool IsCorrect { get; set; }
        public string Message { get; set; }
        public bool IsComplete { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
    }
}

