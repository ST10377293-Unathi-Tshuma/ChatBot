using System;
using System.Collections.Generic;
using System.Linq;

namespace Final_Version.Services
{
    /// <summary>
    /// Enhanced sentiment analyzer with confidence scoring and multiple emotion detection
    /// </summary>
    public class EnhancedSentimentAnalyzer
    {
        private readonly Dictionary<string, List<string>> sentimentKeywords;
        private readonly Dictionary<string, double> keywordWeights;

        public EnhancedSentimentAnalyzer(Dictionary<string, List<string>> sentimentKeywords = null)
        {
            this.sentimentKeywords = sentimentKeywords ?? LoadDefaultKeywords();
            this.keywordWeights = InitializeWeights();
        }

        /// <summary>
        /// Analyzes sentiment with confidence score
        /// </summary>
        public SentimentResult Analyze(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return new SentimentResult { Sentiment = "neutral", Confidence = 0.0 };
            }

            string lowerInput = input.ToLower();
            var scores = new Dictionary<string, double>();

            // Score each sentiment category
            foreach (var sentiment in sentimentKeywords.Keys)
            {
                double score = CalculateSentimentScore(lowerInput, sentiment);
                if (score > 0)
                {
                    scores[sentiment] = score;
                }
            }

            if (scores.Count == 0)
            {
                return new SentimentResult { Sentiment = "neutral", Confidence = 0.0 };
            }

            // Get the sentiment with highest score
            var topSentiment = scores.OrderByDescending(s => s.Value).First();
            double confidence = Math.Min(topSentiment.Value / 10.0, 1.0); // Normalize to 0-1

            return new SentimentResult
            {
                Sentiment = topSentiment.Key,
                Confidence = confidence,
                AllScores = scores
            };
        }

        /// <summary>
        /// Calculates sentiment score for a category
        /// </summary>
        private double CalculateSentimentScore(string input, string sentiment)
        {
            double score = 0.0;
            var keywords = sentimentKeywords[sentiment];

            foreach (var keyword in keywords)
            {
                if (input.Contains(keyword))
                {
                    double weight = keywordWeights.ContainsKey(keyword) 
                        ? keywordWeights[keyword] 
                        : 1.0;

                    // Check for intensifiers (very, extremely, really)
                    if (HasIntensifier(input, keyword))
                    {
                        weight *= 1.5;
                    }

                    // Check for negations (not worried, not scared)
                    if (HasNegation(input, keyword))
                    {
                        weight *= -0.5;
                    }

                    score += weight;
                }
            }

            return score;
        }

        /// <summary>
        /// Checks if keyword has an intensifier nearby
        /// </summary>
        private bool HasIntensifier(string input, string keyword)
        {
            var intensifiers = new[] { "very", "extremely", "really", "quite", "so" };
            int keywordIndex = input.IndexOf(keyword);
            
            if (keywordIndex == -1) return false;

            string beforeKeyword = input.Substring(Math.Max(0, keywordIndex - 15), 
                Math.Min(15, keywordIndex));
            
            return intensifiers.Any(i => beforeKeyword.Contains(i));
        }

        /// <summary>
        /// Checks if keyword is negated
        /// </summary>
        private bool HasNegation(string input, string keyword)
        {
            var negations = new[] { "not", "no", "never", "don't", "doesn't", "isn't" };
            int keywordIndex = input.IndexOf(keyword);
            
            if (keywordIndex == -1) return false;

            string beforeKeyword = input.Substring(Math.Max(0, keywordIndex - 10), 
                Math.Min(10, keywordIndex));
            
            return negations.Any(n => beforeKeyword.Contains(n));
        }

        /// <summary>
        /// Initializes keyword weights (more specific = higher weight)
        /// </summary>
        private Dictionary<string, double> InitializeWeights()
        {
            return new Dictionary<string, double>
            {
                { "worried", 2.0 },
                { "scared", 2.5 },
                { "anxious", 2.0 },
                { "nervous", 1.5 },
                { "curious", 1.5 },
                { "interested", 1.0 },
                { "wondering", 1.0 },
                { "frustrated", 2.0 },
                { "annoyed", 1.5 },
                { "stuck", 1.0 }
            };
        }

        /// <summary>
        /// Loads default sentiment keywords
        /// </summary>
        private Dictionary<string, List<string>> LoadDefaultKeywords()
        {
            return new Dictionary<string, List<string>>
            {
                { "worried", new List<string> { "worried", "scared", "anxious", "nervous", "afraid", "concerned" } },
                { "curious", new List<string> { "curious", "interested", "wondering", "want to know", "tell me" } },
                { "frustrated", new List<string> { "frustrated", "annoyed", "stuck", "confused", "don't understand" } }
            };
        }
    }

    /// <summary>
    /// Result of sentiment analysis
    /// </summary>
    public class SentimentResult
    {
        public string Sentiment { get; set; }
        public double Confidence { get; set; }
        public Dictionary<string, double> AllScores { get; set; }
    }
}

