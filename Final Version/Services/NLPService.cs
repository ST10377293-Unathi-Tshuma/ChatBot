using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Final_Version.Services
{
    /// <summary>
    /// Natural Language Processing service for understanding user intent and extracting meaning
    /// </summary>
    public class NLPService
    {
        private readonly Dictionary<string, List<string>> synonyms;
        private readonly Dictionary<string, List<string>> questionPatterns;
        private readonly Dictionary<string, List<string>> intentPatterns;
        private readonly Random random = new Random();

        public NLPService()
        {
            synonyms = InitializeSynonyms();
            questionPatterns = InitializeQuestionPatterns();
            intentPatterns = InitializeIntentPatterns();
        }

        /// <summary>
        /// Analyzes user input and extracts intent, entities, and context
        /// </summary>
        public NLPResult Analyze(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return new NLPResult { Intent = "unknown", Confidence = 0.0 };
            }

            string normalizedInput = NormalizeInput(input);
            string lowerInput = normalizedInput.ToLower();

            // Extract intent
            var intent = ExtractIntent(lowerInput, normalizedInput);
            
            // Extract entities (keywords, topics, etc.)
            var entities = ExtractEntities(lowerInput);
            
            // Detect question type
            var questionType = DetectQuestionType(lowerInput);
            
            // Extract sentiment indicators
            var sentimentIndicators = ExtractSentimentIndicators(lowerInput);
            
            // Calculate confidence based on multiple factors
            double confidence = CalculateConfidence(intent, entities, lowerInput);

            return new NLPResult
            {
                OriginalInput = input,
                NormalizedInput = normalizedInput,
                Intent = intent,
                Entities = entities,
                QuestionType = questionType,
                SentimentIndicators = sentimentIndicators,
                Confidence = confidence,
                IsQuestion = questionType != QuestionType.None,
                Keywords = ExtractKeywords(lowerInput)
            };
        }

        /// <summary>
        /// Normalizes input text for better matching
        /// </summary>
        private string NormalizeInput(string input)
        {
            // Remove extra whitespace
            input = Regex.Replace(input, @"\s+", " ").Trim();
            
            // Remove punctuation for matching (but keep for context)
            // Convert common contractions
            input = input.Replace("'t", " not")
                         .Replace("'re", " are")
                         .Replace("'ve", " have")
                         .Replace("'ll", " will")
                         .Replace("'d", " would")
                         .Replace("'m", " am")
                         .Replace("'s", " is");
            
            return input;
        }

        /// <summary>
        /// Extracts user intent from input
        /// </summary>
        private string ExtractIntent(string lowerInput, string normalizedInput)
        {
            // Check for explicit commands first
            if (MatchesPattern(lowerInput, intentPatterns["start_quiz"]))
                return "start_quiz";
            
            if (MatchesPattern(lowerInput, intentPatterns["add_task"]))
                return "add_task";
            
            if (MatchesPattern(lowerInput, intentPatterns["view_tasks"]))
                return "view_tasks";
            
            if (MatchesPattern(lowerInput, intentPatterns["delete_task"]))
                return "delete_task";
            
            if (MatchesPattern(lowerInput, intentPatterns["complete_task"]))
                return "complete_task";
            
            if (MatchesPattern(lowerInput, intentPatterns["view_log"]))
                return "view_log";
            
            if (MatchesPattern(lowerInput, intentPatterns["remind_me"]))
                return "remind_me";
            
            // Check for information requests
            if (MatchesPattern(lowerInput, intentPatterns["ask_about"]))
                return "ask_about";
            
            if (MatchesPattern(lowerInput, intentPatterns["explain"]))
                return "explain";
            
            if (MatchesPattern(lowerInput, intentPatterns["how_to"]))
                return "how_to";
            
            if (MatchesPattern(lowerInput, intentPatterns["what_is"]))
                return "what_is";
            
            // Check for follow-up requests
            if (MatchesPattern(lowerInput, intentPatterns["follow_up"]))
                return "follow_up";
            
            // Check for topic preference
            if (MatchesPattern(lowerInput, intentPatterns["set_favorite"]))
                return "set_favorite";
            
            // Check for greetings
            if (MatchesPattern(lowerInput, intentPatterns["greeting"]))
                return "greeting";
            
            // Default to general query
            return "general_query";
        }

        /// <summary>
        /// Extracts entities (topics, keywords) from input
        /// </summary>
        private List<string> ExtractEntities(string lowerInput)
        {
            var entities = new List<string>();
            
            // Extract potential topic keywords
            var topicKeywords = new[] { "password", "phishing", "scam", "privacy", "security", 
                "malware", "firewall", "virus", "hack", "cybersecurity", "encryption", "2fa", 
                "two-factor", "authentication", "browsing", "safe", "data", "breach", "identity" };
            
            foreach (var keyword in topicKeywords)
            {
                if (lowerInput.Contains(keyword))
                {
                    entities.Add(keyword);
                }
            }
            
            // Extract numbers (for task indices, etc.)
            var numbers = Regex.Matches(lowerInput, @"\b\d+\b");
            foreach (Match match in numbers)
            {
                entities.Add(match.Value);
            }
            
            return entities;
        }

        /// <summary>
        /// Detects the type of question being asked
        /// </summary>
        private QuestionType DetectQuestionType(string lowerInput)
        {
            if (lowerInput.StartsWith("what ") || lowerInput.StartsWith("what's ") || 
                lowerInput.StartsWith("what is ") || lowerInput.StartsWith("what are "))
                return QuestionType.What;
            
            if (lowerInput.StartsWith("how ") || lowerInput.StartsWith("how do ") || 
                lowerInput.StartsWith("how can ") || lowerInput.StartsWith("how to "))
                return QuestionType.How;
            
            if (lowerInput.StartsWith("why "))
                return QuestionType.Why;
            
            if (lowerInput.StartsWith("when "))
                return QuestionType.When;
            
            if (lowerInput.StartsWith("where "))
                return QuestionType.Where;
            
            if (lowerInput.StartsWith("who "))
                return QuestionType.Who;
            
            if (lowerInput.Contains("?") || lowerInput.StartsWith("can ") || 
                lowerInput.StartsWith("should ") || lowerInput.StartsWith("is ") ||
                lowerInput.StartsWith("are ") || lowerInput.StartsWith("do ") ||
                lowerInput.StartsWith("does "))
                return QuestionType.YesNo;
            
            return QuestionType.None;
        }

        /// <summary>
        /// Extracts sentiment indicators from input
        /// </summary>
        private List<string> ExtractSentimentIndicators(string lowerInput)
        {
            var indicators = new List<string>();
            
            var sentimentWords = new Dictionary<string, string[]>
            {
                { "worried", new[] { "worried", "concerned", "anxious", "nervous", "afraid", "scared" } },
                { "curious", new[] { "curious", "interested", "wondering", "want to know", "tell me" } },
                { "frustrated", new[] { "frustrated", "annoyed", "stuck", "confused", "don't understand", "can't figure out" } },
                { "excited", new[] { "excited", "eager", "enthusiastic", "looking forward" } },
                { "grateful", new[] { "thanks", "thank you", "appreciate", "helpful" } }
            };
            
            foreach (var sentiment in sentimentWords)
            {
                if (sentiment.Value.Any(word => lowerInput.Contains(word)))
                {
                    indicators.Add(sentiment.Key);
                }
            }
            
            return indicators;
        }

        /// <summary>
        /// Extracts keywords from input using fuzzy matching
        /// </summary>
        private List<string> ExtractKeywords(string lowerInput)
        {
            var keywords = new List<string>();
            var words = lowerInput.Split(new[] { ' ', ',', '.', '!', '?' }, 
                StringSplitOptions.RemoveEmptyEntries);
            
            // Filter out common stop words
            var stopWords = new HashSet<string> { "the", "a", "an", "and", "or", "but", "in", 
                "on", "at", "to", "for", "of", "with", "by", "is", "are", "was", "were", 
                "be", "been", "have", "has", "had", "do", "does", "did", "will", "would", 
                "can", "could", "should", "may", "might", "must", "this", "that", "these", 
                "those", "i", "you", "he", "she", "it", "we", "they", "me", "him", "her", 
                "us", "them", "my", "your", "his", "her", "its", "our", "their", "what", 
                "which", "who", "whom", "whose", "where", "when", "why", "how", "about", 
                "tell", "me", "please", "thanks", "thank" };
            
            foreach (var word in words)
            {
                string cleanWord = word.Trim().ToLower();
                if (!stopWords.Contains(cleanWord) && cleanWord.Length > 2)
                {
                    keywords.Add(cleanWord);
                }
            }
            
            return keywords;
        }

        /// <summary>
        /// Matches input against a list of patterns
        /// </summary>
        private bool MatchesPattern(string input, List<string> patterns)
        {
            return patterns.Any(pattern => 
            {
                // Exact match
                if (input == pattern)
                    return true;
                
                // Contains match
                if (input.Contains(pattern))
                    return true;
                
                // Word boundary match (whole word)
                string wordPattern = @"\b" + Regex.Escape(pattern) + @"\b";
                if (Regex.IsMatch(input, wordPattern, RegexOptions.IgnoreCase))
                    return true;
                
                return false;
            });
        }

        /// <summary>
        /// Calculates confidence score for the analysis
        /// </summary>
        private double CalculateConfidence(string intent, List<string> entities, string input)
        {
            double confidence = 0.5; // Base confidence
            
            // Increase confidence if entities found
            if (entities.Count > 0)
                confidence += 0.2;
            
            // Increase confidence for specific intents
            if (intent != "general_query" && intent != "unknown")
                confidence += 0.2;
            
            // Increase confidence if input has question markers
            if (input.Contains("?") || input.StartsWith("what") || input.StartsWith("how") || 
                input.StartsWith("why") || input.StartsWith("when") || input.StartsWith("where"))
                confidence += 0.1;
            
            return Math.Min(confidence, 1.0);
        }

        /// <summary>
        /// Finds semantic matches for a keyword using synonyms
        /// </summary>
        public List<string> FindSemanticMatches(string keyword)
        {
            var matches = new List<string> { keyword.ToLower() };
            
            foreach (var synonymGroup in synonyms)
            {
                if (synonymGroup.Value.Contains(keyword.ToLower()))
                {
                    matches.AddRange(synonymGroup.Value);
                }
            }
            
            return matches.Distinct().ToList();
        }

        /// <summary>
        /// Checks if input matches a keyword considering synonyms
        /// </summary>
        public bool MatchesKeyword(string input, string keyword)
        {
            string lowerInput = input.ToLower();
            string lowerKeyword = keyword.ToLower();
            
            // Direct match
            if (lowerInput.Contains(lowerKeyword))
                return true;
            
            // Synonym match
            var semanticMatches = FindSemanticMatches(lowerKeyword);
            return semanticMatches.Any(match => lowerInput.Contains(match));
        }

        /// <summary>
        /// Initializes synonym dictionary
        /// </summary>
        private Dictionary<string, List<string>> InitializeSynonyms()
        {
            return new Dictionary<string, List<string>>
            {
                { "password", new List<string> { "password", "passcode", "pin", "passphrase", "credentials", "login" } },
                { "phishing", new List<string> { "phishing", "scam", "fraud", "hoax", "trick", "deception" } },
                { "privacy", new List<string> { "privacy", "private", "confidential", "personal", "secret" } },
                { "security", new List<string> { "security", "secure", "safety", "protection", "safe", "defense" } },
                { "malware", new List<string> { "malware", "virus", "trojan", "spyware", "ransomware", "adware" } },
                { "firewall", new List<string> { "firewall", "barrier", "protection", "shield" } },
                { "hack", new List<string> { "hack", "breach", "attack", "intrusion", "compromise", "exploit" } },
                { "encryption", new List<string> { "encryption", "encrypt", "encrypted", "cipher", "encode" } },
                { "authentication", new List<string> { "authentication", "auth", "login", "verify", "2fa", "two-factor", "mfa" } },
                { "browsing", new List<string> { "browsing", "surfing", "web", "internet", "online" } }
            };
        }

        /// <summary>
        /// Initializes question pattern dictionary
        /// </summary>
        private Dictionary<string, List<string>> InitializeQuestionPatterns()
        {
            return new Dictionary<string, List<string>>
            {
                { "what_is", new List<string> { "what is", "what's", "what are", "define", "definition", "explain" } },
                { "how_to", new List<string> { "how to", "how do", "how can", "how should", "steps to", "way to" } },
                { "why", new List<string> { "why", "reason", "because", "cause" } },
                { "when", new List<string> { "when", "time", "schedule" } },
                { "where", new List<string> { "where", "location", "place" } },
                { "who", new List<string> { "who", "person", "people" } }
            };
        }

        /// <summary>
        /// Initializes intent pattern dictionary
        /// </summary>
        private Dictionary<string, List<string>> InitializeIntentPatterns()
        {
            return new Dictionary<string, List<string>>
            {
                { "start_quiz", new List<string> { "start quiz", "begin quiz", "take quiz", "quiz me", "test me" } },
                { "add_task", new List<string> { "add task", "create task", "new task", "task:", "remind me to" } },
                { "view_tasks", new List<string> { "view tasks", "show tasks", "list tasks", "my tasks", "tasks" } },
                { "delete_task", new List<string> { "delete task", "remove task", "cancel task" } },
                { "complete_task", new List<string> { "complete task", "finish task", "done task", "task done" } },
                { "view_log", new List<string> { "view log", "show log", "activity log", "my log" } },
                { "remind_me", new List<string> { "remind me", "set reminder", "reminder for" } },
                { "ask_about", new List<string> { "tell me about", "what about", "info about", "information on", "learn about" } },
                { "explain", new List<string> { "explain", "describe", "elaborate", "details", "more about" } },
                { "how_to", new List<string> { "how to", "how do i", "how can i", "steps", "guide" } },
                { "what_is", new List<string> { "what is", "what's", "what are", "define" } },
                { "follow_up", new List<string> { "tell me more", "more info", "more information", "expand", "elaborate", "go on", "continue" } },
                { "set_favorite", new List<string> { "favorite topic", "interested in", "prefer", "like", "favorite is" } },
                { "greeting", new List<string> { "hello", "hi", "hey", "greetings", "good morning", "good afternoon", "good evening" } }
            };
        }
    }

    /// <summary>
    /// Result of NLP analysis
    /// </summary>
    public class NLPResult
    {
        public string OriginalInput { get; set; }
        public string NormalizedInput { get; set; }
        public string Intent { get; set; }
        public List<string> Entities { get; set; } = new List<string>();
        public QuestionType QuestionType { get; set; }
        public List<string> SentimentIndicators { get; set; } = new List<string>();
        public double Confidence { get; set; }
        public bool IsQuestion { get; set; }
        public List<string> Keywords { get; set; } = new List<string>();
    }

    /// <summary>
    /// Types of questions
    /// </summary>
    public enum QuestionType
    {
        None,
        What,
        How,
        Why,
        When,
        Where,
        Who,
        YesNo
    }
}

