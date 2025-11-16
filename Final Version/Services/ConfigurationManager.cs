using System;
using System.Collections.Generic;
using System.Linq;

namespace Final_Version.Services
{
    /// <summary>
    /// Manages bot configuration loaded from external files
    /// </summary>
    public class ConfigurationManager
    {
        private readonly BotConfiguration configuration;
        private readonly DataLoader dataLoader;

        public ConfigurationManager()
        {
            dataLoader = new DataLoader();
            configuration = dataLoader.LoadConfiguration();
        }

        public BotConfiguration Configuration => configuration;

        /// <summary>
        /// Gets command aliases for a command type
        /// </summary>
        public List<string> GetCommandAliases(string commandType)
        {
            if (configuration.Commands.ContainsKey(commandType))
            {
                return configuration.Commands[commandType];
            }
            return new List<string>();
        }

        /// <summary>
        /// Checks if input matches any alias for a command
        /// </summary>
        public bool MatchesCommand(string input, string commandType)
        {
            var aliases = GetCommandAliases(commandType);
            string lowerInput = input.ToLower().Trim();
            
            foreach (var alias in aliases)
            {
                if (lowerInput == alias || lowerInput.StartsWith(alias))
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Gets sentiment keywords for a sentiment type
        /// </summary>
        public List<string> GetSentimentKeywords(string sentimentType)
        {
            if (configuration.SentimentKeywords.ContainsKey(sentimentType))
            {
                return configuration.SentimentKeywords[sentimentType];
            }
            return new List<string>();
        }

        /// <summary>
        /// Gets all follow-up keywords
        /// </summary>
        public List<string> GetFollowUpKeywords()
        {
            return configuration.FollowUpKeywords;
        }

        /// <summary>
        /// Gets default user name
        /// </summary>
        public string GetDefaultUserName()
        {
            return configuration.Settings.DefaultUserName;
        }

        /// <summary>
        /// Gets default favorite topic
        /// </summary>
        public string GetDefaultFavoriteTopic()
        {
            return configuration.Settings.DefaultFavoriteTopic;
        }

        /// <summary>
        /// Checks if sentiment analysis is enabled
        /// </summary>
        public bool IsSentimentAnalysisEnabled()
        {
            return configuration.Settings.EnableSentimentAnalysis;
        }
    }
}

