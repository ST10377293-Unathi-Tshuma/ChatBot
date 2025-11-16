using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Final_Version.Models;

namespace Final_Version.Services
{
    /// <summary>
    /// Loads bot data from external JSON files instead of hardcoded values
    /// </summary>
    public class DataLoader
    {
        private const string DataFolder = "Data";
        private const string TopicsFile = "TopicResponses.json";
        private const string QuizFile = "QuizQuestions.json";
        private const string KnowledgeFile = "GeneralKnowledge.json";
        private const string ConfigFile = "BotConfig.json";

        /// <summary>
        /// Loads topic responses from JSON file
        /// </summary>
        public Dictionary<string, TopicData> LoadTopics()
        {
            try
            {
                string filePath = Path.Combine(DataFolder, TopicsFile);
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    return ParseTopicsJson(json);
                }
            }
            catch (Exception ex)
            {
                // Log error and fallback to default
                System.Diagnostics.Debug.WriteLine($"Error loading topics: {ex.Message}");
            }
            
            // Return empty dictionary - caller should handle fallback
            return new Dictionary<string, TopicData>();
        }

        /// <summary>
        /// Loads quiz questions from JSON file
        /// </summary>
        public List<QuizQuestion> LoadQuizQuestions()
        {
            try
            {
                string filePath = Path.Combine(DataFolder, QuizFile);
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    return ParseQuizJson(json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading quiz questions: {ex.Message}");
            }
            
            return new List<QuizQuestion>();
        }

        /// <summary>
        /// Loads general knowledge from JSON file
        /// </summary>
        public Dictionary<string, string> LoadGeneralKnowledge()
        {
            try
            {
                string filePath = Path.Combine(DataFolder, KnowledgeFile);
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    return ParseKnowledgeJson(json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading knowledge: {ex.Message}");
            }
            
            return new Dictionary<string, string>();
        }

        /// <summary>
        /// Loads bot configuration from JSON file
        /// </summary>
        public BotConfiguration LoadConfiguration()
        {
            try
            {
                string filePath = Path.Combine(DataFolder, ConfigFile);
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    return ParseConfigJson(json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading configuration: {ex.Message}");
            }
            
            // Return default configuration
            return new BotConfiguration();
        }

        #region JSON Parsing Methods

        private Dictionary<string, TopicData> ParseTopicsJson(string json)
        {
            var result = new Dictionary<string, TopicData>();
            
            // Simple JSON parsing for topics
            json = json.Trim();
            if (!json.StartsWith("{") || !json.Contains("\"topics\""))
                return result;

            // Extract topics object
            int topicsStart = json.IndexOf("\"topics\"") + 8;
            int topicsEnd = json.LastIndexOf("}");
            if (topicsStart < 8 || topicsEnd <= topicsStart)
                return result;

            string topicsJson = json.Substring(topicsStart, topicsEnd - topicsStart);
            topicsJson = topicsJson.Trim().TrimStart(':').TrimStart('{').TrimEnd('}');

            // Parse each topic
            var topics = SplitJsonObjects(topicsJson);
            foreach (var topic in topics)
            {
                var topicData = ParseTopicObject(topic);
                if (topicData.HasValue)
                {
                    result[topicData.Value.Key] = topicData.Value.Value;
                }
            }

            return result;
        }

        private KeyValuePair<string, TopicData>? ParseTopicObject(string json)
        {
            try
            {
                // Extract key (topic name)
                int keyStart = json.IndexOf('"') + 1;
                int keyEnd = json.IndexOf('"', keyStart);
                if (keyStart <= 0 || keyEnd <= keyStart)
                    return null;

                string key = json.Substring(keyStart, keyEnd - keyStart);

                // Extract value object
                int valueStart = json.IndexOf('{', keyEnd);
                int valueEnd = json.LastIndexOf('}');
                if (valueStart < 0 || valueEnd <= valueStart)
                    return null;

                string valueJson = json.Substring(valueStart, valueEnd - valueStart + 1);
                var topicData = ParseTopicData(valueJson);
                
                return new KeyValuePair<string, TopicData>(key, topicData);
            }
            catch
            {
                return null;
            }
        }

        private TopicData ParseTopicData(string json)
        {
            var topicData = new TopicData();

            // Extract name
            if (json.Contains("\"name\""))
            {
                topicData.Name = ExtractStringValue(json, "name");
            }

            // Extract keywords
            if (json.Contains("\"keywords\""))
            {
                topicData.Keywords = ExtractStringArray(json, "keywords");
            }

            // Extract responses
            if (json.Contains("\"responses\""))
            {
                topicData.Responses = ExtractStringArray(json, "responses");
            }

            return topicData;
        }

        private List<QuizQuestion> ParseQuizJson(string json)
        {
            var questions = new List<QuizQuestion>();

            if (!json.Contains("\"questions\""))
                return questions;

            // Extract questions array
            int questionsStart = json.IndexOf('[');
            int questionsEnd = json.LastIndexOf(']');
            if (questionsStart < 0 || questionsEnd <= questionsStart)
                return questions;

            string questionsJson = json.Substring(questionsStart + 1, questionsEnd - questionsStart - 1);
            var questionObjects = SplitJsonObjects(questionsJson);

            foreach (var questionJson in questionObjects)
            {
                var question = ParseQuizQuestion(questionJson);
                if (question != null)
                {
                    questions.Add(question);
                }
            }

            return questions;
        }

        private QuizQuestion ParseQuizQuestion(string json)
        {
            try
            {
                var question = new QuizQuestion();
                question.Question = ExtractStringValue(json, "question");
                question.Options = ExtractStringArray(json, "options").ToArray();
                
                string correctIndexStr = ExtractStringValue(json, "correctAnswerIndex");
                if (int.TryParse(correctIndexStr, out int correctIndex))
                {
                    question.CorrectAnswerIndex = correctIndex;
                }

                question.Explanation = ExtractStringValue(json, "explanation");
                return question;
            }
            catch
            {
                return null;
            }
        }

        private Dictionary<string, string> ParseKnowledgeJson(string json)
        {
            var result = new Dictionary<string, string>();

            if (!json.Contains("\"knowledge\""))
                return result;

            int knowledgeStart = json.IndexOf("\"knowledge\"") + 11;
            int knowledgeEnd = json.LastIndexOf("}");
            if (knowledgeStart < 11 || knowledgeEnd <= knowledgeStart)
                return result;

            string knowledgeJson = json.Substring(knowledgeStart, knowledgeEnd - knowledgeStart);
            knowledgeJson = knowledgeJson.Trim().TrimStart(':').TrimStart('{').TrimEnd('}');

            var knowledgeItems = SplitJsonObjects(knowledgeJson);
            foreach (var item in knowledgeItems)
            {
                int keyStart = item.IndexOf('"') + 1;
                int keyEnd = item.IndexOf('"', keyStart);
                if (keyStart <= 0 || keyEnd <= keyStart)
                    continue;

                string key = item.Substring(keyStart, keyEnd - keyStart);
                string value = ExtractStringValue(item, key);
                if (!string.IsNullOrEmpty(value))
                {
                    result[key] = value;
                }
            }

            return result;
        }

        private BotConfiguration ParseConfigJson(string json)
        {
            var config = new BotConfiguration();
            config.Settings = new BotSettings();
            config.Commands = new Dictionary<string, List<string>>();
            config.SentimentKeywords = new Dictionary<string, List<string>>();
            config.FollowUpKeywords = new List<string>();

            // Parse settings
            if (json.Contains("\"settings\""))
            {
                string settingsJson = ExtractObject(json, "settings");
                config.Settings.DefaultUserName = ExtractStringValue(settingsJson, "defaultUserName") ?? "User";
                config.Settings.DefaultFavoriteTopic = ExtractStringValue(settingsJson, "defaultFavoriteTopic") ?? "privacy";
                
                string maxLengthStr = ExtractStringValue(settingsJson, "maxResponseLength");
                if (int.TryParse(maxLengthStr, out int maxLength))
                    config.Settings.MaxResponseLength = maxLength;
            }

            // Parse commands
            if (json.Contains("\"commands\""))
            {
                string commandsJson = ExtractObject(json, "commands");
                var commandKeys = ExtractKeys(commandsJson);
                foreach (var key in commandKeys)
                {
                    var aliases = ExtractStringArray(commandsJson, key);
                    config.Commands[key] = aliases;
                }
            }

            // Parse sentiment keywords
            if (json.Contains("\"sentimentKeywords\""))
            {
                string sentimentJson = ExtractObject(json, "sentimentKeywords");
                var sentimentKeys = ExtractKeys(sentimentJson);
                foreach (var key in sentimentKeys)
                {
                    var keywords = ExtractStringArray(sentimentJson, key);
                    config.SentimentKeywords[key] = keywords;
                }
            }

            // Parse follow-up keywords
            if (json.Contains("\"followUpKeywords\""))
            {
                config.FollowUpKeywords = ExtractStringArray(json, "followUpKeywords");
            }

            return config;
        }

        #endregion

        #region Helper Methods

        private List<string> SplitJsonObjects(string json)
        {
            var objects = new List<string>();
            int depth = 0;
            int start = 0;

            for (int i = 0; i < json.Length; i++)
            {
                if (json[i] == '{')
                    depth++;
                else if (json[i] == '}')
                    depth--;
                else if (json[i] == ',' && depth == 0)
                {
                    objects.Add(json.Substring(start, i - start).Trim());
                    start = i + 1;
                }
            }

            if (start < json.Length)
            {
                objects.Add(json.Substring(start).Trim());
            }

            return objects;
        }

        private string ExtractStringValue(string json, string key)
        {
            string searchKey = "\"" + key + "\"";
            int keyIndex = json.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase);
            if (keyIndex < 0)
                return null;

            int valueStart = json.IndexOf(':', keyIndex) + 1;
            valueStart = SkipWhitespace(json, valueStart);

            if (valueStart >= json.Length)
                return null;

            if (json[valueStart] == '"')
            {
                // String value
                int stringStart = valueStart + 1;
                int stringEnd = FindStringEnd(json, stringStart);
                if (stringEnd > stringStart)
                {
                    return json.Substring(stringStart, stringEnd - stringStart);
                }
            }
            else if (char.IsDigit(json[valueStart]) || json[valueStart] == '-')
            {
                // Number value
                int numEnd = valueStart;
                while (numEnd < json.Length && (char.IsDigit(json[numEnd]) || json[numEnd] == '.' || json[numEnd] == '-'))
                    numEnd++;
                return json.Substring(valueStart, numEnd - valueStart);
            }

            return null;
        }

        private List<string> ExtractStringArray(string json, string key)
        {
            var result = new List<string>();
            string searchKey = "\"" + key + "\"";
            int keyIndex = json.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase);
            if (keyIndex < 0)
                return result;

            int arrayStart = json.IndexOf('[', keyIndex);
            if (arrayStart < 0)
                return result;

            int arrayEnd = json.IndexOf(']', arrayStart);
            if (arrayEnd <= arrayStart)
                return result;

            string arrayJson = json.Substring(arrayStart + 1, arrayEnd - arrayStart - 1);
            var items = arrayJson.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var item in items)
            {
                string trimmed = item.Trim().Trim('"');
                if (!string.IsNullOrEmpty(trimmed))
                {
                    result.Add(trimmed);
                }
            }

            return result;
        }

        private string ExtractObject(string json, string key)
        {
            string searchKey = "\"" + key + "\"";
            int keyIndex = json.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase);
            if (keyIndex < 0)
                return "";

            int objStart = json.IndexOf('{', keyIndex);
            if (objStart < 0)
                return "";

            int depth = 0;
            int objEnd = objStart;
            for (int i = objStart; i < json.Length; i++)
            {
                if (json[i] == '{')
                    depth++;
                else if (json[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        objEnd = i + 1;
                        break;
                    }
                }
            }

            if (objEnd > objStart)
            {
                return json.Substring(objStart, objEnd - objStart);
            }

            return "";
        }

        private List<string> ExtractKeys(string json)
        {
            var keys = new List<string>();
            int i = 0;
            while (i < json.Length)
            {
                if (json[i] == '"')
                {
                    int keyStart = i + 1;
                    int keyEnd = json.IndexOf('"', keyStart);
                    if (keyEnd > keyStart)
                    {
                        keys.Add(json.Substring(keyStart, keyEnd - keyStart));
                        i = keyEnd + 1;
                    }
                    else
                    {
                        i++;
                    }
                }
                else
                {
                    i++;
                }
            }
            return keys;
        }

        private int SkipWhitespace(string json, int start)
        {
            while (start < json.Length && char.IsWhiteSpace(json[start]))
                start++;
            return start;
        }

        private int FindStringEnd(string json, int start)
        {
            for (int i = start; i < json.Length; i++)
            {
                if (json[i] == '"' && (i == start || json[i - 1] != '\\'))
                    return i;
            }
            return json.Length;
        }

        #endregion
    }

    #region Data Models

    public class TopicData
    {
        public string Name { get; set; }
        public List<string> Keywords { get; set; } = new List<string>();
        public List<string> Responses { get; set; } = new List<string>();
    }

    public class BotConfiguration
    {
        public BotSettings Settings { get; set; } = new BotSettings();
        public Dictionary<string, List<string>> Commands { get; set; } = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> SentimentKeywords { get; set; } = new Dictionary<string, List<string>>();
        public List<string> FollowUpKeywords { get; set; } = new List<string>();
    }

    public class BotSettings
    {
        public string DefaultUserName { get; set; } = "User";
        public string DefaultFavoriteTopic { get; set; } = "privacy";
        public int MaxResponseLength { get; set; } = 500;
        public bool EnableSentimentAnalysis { get; set; } = true;
        public bool EnableLearning { get; set; } = false;
        public string ResponseStyle { get; set; } = "friendly";
    }

    #endregion
}

