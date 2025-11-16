using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Final_Version.Services
{
    /// <summary>
    /// Generates responses based on user input, topics, and sentiment
    /// </summary>
    public class ResponseGenerator
    {
        private readonly Random random = new Random();
        private readonly DataLoader dataLoader;
        private string currentTopic = null;

        private Dictionary<string, TopicData> topicData;
        private Dictionary<string, List<string>> topicResponses;
        private Dictionary<string, string> keywordToTopic;
        private Dictionary<string, string> generalKnowledge;

        public Dictionary<string, List<string>> TopicResponses => topicResponses;
        public Dictionary<string, string> GeneralKnowledge => generalKnowledge;
        public Dictionary<string, string> KeywordToTopic => keywordToTopic;

        public string CurrentTopic => currentTopic;

        public ResponseGenerator()
        {
            dataLoader = new DataLoader();
            LoadData();
        }

        /// <summary>
        /// Loads data from external JSON files
        /// </summary>
        private void LoadData()
        {
            // Load topic data
            topicData = dataLoader.LoadTopics();
            
            // Convert to internal format
            topicResponses = new Dictionary<string, List<string>>();
            keywordToTopic = new Dictionary<string, string>();

            foreach (var kvp in topicData)
            {
                string topicKey = kvp.Key;
                TopicData data = kvp.Value;
                
                // Store responses
                topicResponses[topicKey] = data.Responses;
                
                // Map keywords to topic
                foreach (var keyword in data.Keywords)
                {
                    keywordToTopic[keyword] = topicKey;
                }
            }

            // Load general knowledge
            generalKnowledge = dataLoader.LoadGeneralKnowledge();

            // Fallback to hardcoded data if loading failed
            if (topicResponses.Count == 0)
            {
                LoadFallbackData();
            }
        }

        /// <summary>
        /// Fallback to hardcoded data if JSON loading fails
        /// </summary>
        private void LoadFallbackData()
        {
            topicResponses = new Dictionary<string, List<string>>
            {
                {
                    "password_safety",
                    new List<string>
                    {
                        "Always use strong, unique passwords for each account. A strong password should be at least 12 characters long, include a mix of uppercase letters, lowercase letters, numbers, and symbols, and avoid personal information like your name or birthdate. For example, instead of 'John123', use something like 'Tr0ub4dor&3xplor3r'. Consider using a password manager to generate and store complex passwords securely.",
                        "Make sure your passwords are complex and unique for every account. Aim for at least 12 characters with a combination of letters, numbers, and symbols, like 'P@ssw0rd#2025'. Avoid using easily guessable info, such as your birthday, and never reuse passwords—if one account is hacked, others could be at risk too.",
                        "Create strong passwords by using a mix of characters—uppercase, lowercase, numbers, and symbols—and make them at least 12 characters long. For instance, 'G7m!x9qL$2vP' is a good example. Don't use the same password across multiple sites, and consider a password manager to keep track of them securely."
                    }
                },
                {
                    "phishing_scams",
                    new List<string>
                    {
                        "Be cautious of emails asking for personal information. Scammers often disguise themselves as trusted organizations. For example, you might get an email saying your account is locked and asking you to click a link—don't! Always verify the sender's email address and contact the organization directly if unsure.",
                        "Phishing scams trick you into sharing sensitive info, like passwords, by posing as legitimate sources. For instance, a fake email might claim you've won a prize and ask for your bank details. Never click links in unsolicited messages, and double-check the sender's email for typos, like 'support@yourbannk.com'.",
                        "Scammers often use phishing to steal your data through fake emails or texts. An example is a message claiming your account needs 'urgent verification' with a link to a fake login page. Always hover over links to check the URL before clicking, and avoid sharing personal info with unknown contacts."
                    }
                },
                {
                    "safe_browsing_privacy",
                    new List<string>
                    {
                        "Protecting your privacy online starts with safe browsing habits. Always check that websites use HTTPS (look for the padlock icon in the browser), which ensures your data is encrypted. For example, 'https://www.example.com' is safer than 'http://example.com'. Be cautious about sharing personal information—avoid posting sensitive details like your address or phone number on social media.",
                        "Keep your online activity private by ensuring websites use HTTPS—check for the padlock in your browser. For instance, 'https://www.google.com' is secure, but 'http://' isn't. Use privacy settings on social media to limit who can see your posts, and enable two-factor authentication (2FA) for extra security.",
                        "Stay safe online by browsing securely—always use HTTPS websites, which encrypt your data (e.g., 'https://www.example.com'). Don't share personal details like your phone number publicly, and clear your browser cookies regularly to prevent tracking. Adding 2FA to your accounts also helps protect your privacy."
                    }
                }
            };

            keywordToTopic = new Dictionary<string, string>
            {
                { "password", "password_safety" },
                { "passcode", "password_safety" },
                { "login", "password_safety" },
                { "credentials", "password_safety" },
                { "passwords", "password_safety" },
                { "scam", "phishing_scams" },
                { "phishing", "phishing_scams" },
                { "fraud", "phishing_scams" },
                { "hoax", "phishing_scams" },
                { "scamming", "phishing_scams" },
                { "privacy", "safe_browsing_privacy" },
                { "security", "safe_browsing_privacy" },
                { "browsing", "safe_browsing_privacy" },
                { "data", "safe_browsing_privacy" },
                { "private", "safe_browsing_privacy" },
                { "2fa", "safe_browsing_privacy" },
                { "two-factor", "safe_browsing_privacy" }
            };

            generalKnowledge = new Dictionary<string, string>
            {
                { "cybersecurity", "Cybersecurity is the practice of protecting computers, servers, mobile devices, electronic systems, networks, and data from digital attacks, unauthorized access, or damage. It involves a range of practices, like using strong passwords, enabling two-factor authentication, and keeping software updated. For example, a company might use firewalls and encryption to secure its data, while individuals can protect themselves by avoiding suspicious links and using antivirus software." },
                { "firewall", "A firewall is a security system that monitors and controls incoming and outgoing network traffic based on predefined rules. It acts like a barrier between your device and potential threats on the internet. For instance, a firewall might block unauthorized access to your computer while allowing safe connections, like accessing a trusted website. Firewalls are essential for both personal devices and business networks to prevent cyberattacks." },
                { "malware", "Malware, short for malicious software, is any program designed to harm or exploit a device, network, or user. Common types include viruses, worms, ransomware, and spyware. For example, ransomware can lock your files and demand payment to unlock them, while spyware might secretly track your activity. To protect yourself, always avoid downloading files from untrusted sources, keep your antivirus software updated, and be cautious with email attachments." }
            };
        }

        /// <summary>
        /// Detects sentiment from user input
        /// </summary>
        public string DetectSentiment(string input)
        {
            if (input.Contains("worried") || input.Contains("scared") || input.Contains("anxious") || input.Contains("nervous"))
                return "worried";
            if (input.Contains("curious") || input.Contains("interested") || input.Contains("wondering"))
                return "curious";
            if (input.Contains("frustrated") || input.Contains("annoyed") || input.Contains("stuck"))
                return "frustrated";
            return null;
        }

        /// <summary>
        /// Checks if input is a follow-up request
        /// </summary>
        public bool IsFollowUpRequest(string lowerInput, List<string> followUpKeywords)
        {
            return currentTopic != null && followUpKeywords.Any(keyword => 
                lowerInput == keyword || lowerInput.Contains(keyword));
        }

        /// <summary>
        /// Gets a response for a topic with NLP-aware matching
        /// </summary>
        public string GetTopicResponse(string topicKey, NLPService nlpService = null, string userInput = null)
        {
            if (string.IsNullOrEmpty(topicKey) || !topicResponses.ContainsKey(topicKey))
            {
                return null;
            }

            var responses = topicResponses[topicKey];
            string baseResponse = responses[random.Next(responses.Count)];

            // Enhance response with NLP if available
            if (nlpService != null && !string.IsNullOrEmpty(userInput))
            {
                baseResponse = EnhanceResponseWithNLP(baseResponse, userInput, nlpService);
            }

            return baseResponse;
        }

        /// <summary>
        /// Enhances response using NLP insights
        /// </summary>
        private string EnhanceResponseWithNLP(string response, string userInput, NLPService nlpService)
        {
            // If user asked a "how to" question, make response more action-oriented
            var nlpResult = nlpService.Analyze(userInput);
            if (nlpResult.QuestionType == QuestionType.How)
            {
                // Ensure response has actionable steps
                if (!response.Contains("step") && !response.Contains("first") && !response.Contains("then"))
                {
                    response = response.Replace(". ", ". Here's how: ");
                }
            }

            return response;
        }

        /// <summary>
        /// Gets a general knowledge response
        /// </summary>
        public string GetGeneralKnowledgeResponse(string lowerInput)
        {
            var matchingKey = generalKnowledge.Keys.FirstOrDefault(key => lowerInput.Contains(key));
            if (matchingKey != null)
            {
                currentTopic = null;
                return generalKnowledge[matchingKey];
            }
            return null;
        }

        /// <summary>
        /// Gets a topic response based on keywords with semantic matching
        /// </summary>
        public string GetResponseFromKeywords(string lowerInput, NLPService nlpService = null)
        {
            // Use semantic matching if NLP service is available
            if (nlpService != null)
            {
                foreach (var keyword in keywordToTopic.Keys)
                {
                    if (nlpService.MatchesKeyword(lowerInput, keyword))
                    {
                        currentTopic = keyword;
                        string topicKey = keywordToTopic[keyword];
                        return GetTopicResponse(topicKey, nlpService, lowerInput);
                    }
                }
            }
            else
            {
                // Fallback to simple keyword matching
                var matchingKeyword = keywordToTopic.Keys.FirstOrDefault(keyword => lowerInput.Contains(keyword));
                if (matchingKeyword != null)
                {
                    currentTopic = matchingKeyword;
                    string topicKey = keywordToTopic[matchingKeyword];
                    return GetTopicResponse(topicKey);
                }
            }
            return null;
        }

        /// <summary>
        /// Gets topic name from topic key
        /// </summary>
        public string GetTopicName(string topicKey)
        {
            if (topicData != null && topicData.ContainsKey(topicKey))
            {
                return topicData[topicKey].Name;
            }

            // Fallback to hardcoded names
            if (topicKey == "password_safety") return "Password Safety";
            if (topicKey == "phishing_scams") return "Phishing and Scams";
            if (topicKey == "safe_browsing_privacy") return "Safe Browsing and Privacy";
            return "this topic";
        }

        /// <summary>
        /// Gets topic key from keyword
        /// </summary>
        public string GetTopicKey(string keyword)
        {
            return keywordToTopic.ContainsKey(keyword) ? keywordToTopic[keyword] : null;
        }

        /// <summary>
        /// Adjusts response based on sentiment and context with more natural language
        /// </summary>
        public string AdjustResponse(string baseResponse, string sentiment, bool isFollowUp, string userName, 
            NLPResult nlpResult = null, ConversationContext context = null)
        {
            if (string.IsNullOrEmpty(baseResponse)) return null;

            // Generate natural opening based on context
            string opening = GenerateNaturalOpening(userName, sentiment, isFollowUp, nlpResult, context);
            
            // Generate natural closing
            string closing = GenerateNaturalClosing(sentiment, isFollowUp, nlpResult);

            // Combine into fluent response
            return $"{opening}{baseResponse}{closing}";
        }

        /// <summary>
        /// Generates a natural opening phrase for responses
        /// </summary>
        private string GenerateNaturalOpening(string userName, string sentiment, bool isFollowUp, 
            NLPResult nlpResult, ConversationContext context)
        {
            var openings = new List<string>();

            if (isFollowUp)
            {
                string topicName = GetTopicName(GetTopicKey(currentTopic));
                openings.AddRange(new[]
                {
                    $"Absolutely, {userName}! Let's explore {topicName} in more detail. ",
                    $"Great question! Here's more about {topicName}: ",
                    $"I'd be happy to elaborate on {topicName}, {userName}. ",
                    $"Sure thing! {topicName} is really important. "
                });
            }
            else if (nlpResult != null && nlpResult.IsQuestion)
            {
                switch (nlpResult.QuestionType)
                {
                    case QuestionType.What:
                        openings.AddRange(new[]
                        {
                            $"That's a great question, {userName}! ",
                            $"I'm happy to explain that, {userName}. ",
                            $"Sure, {userName}! Here's what you need to know: "
                        });
                        break;
                    case QuestionType.How:
                        openings.AddRange(new[]
                        {
                            $"Great question! Here's how you can do that, {userName}: ",
                            $"I'll walk you through it, {userName}: ",
                            $"Here's a step-by-step approach, {userName}: "
                        });
                        break;
                    case QuestionType.Why:
                        openings.AddRange(new[]
                        {
                            $"That's an important question, {userName}. ",
                            $"Good thinking! Here's why, {userName}: ",
                            $"Let me explain the reasoning, {userName}: "
                        });
                        break;
                    default:
                        openings.Add($"{userName}, here's what I can tell you: ");
                        break;
                }
            }
            else
            {
                // Sentiment-based openings
                switch (sentiment)
                {
                    case "worried":
                        openings.AddRange(new[]
                        {
                            $"{userName}, I completely understand your concern. ",
                            $"It's totally normal to feel that way, {userName}. ",
                            $"I hear you, {userName}, and that's a valid concern. "
                        });
                        break;
                    case "curious":
                        openings.AddRange(new[]
                        {
                            $"I love your curiosity, {userName}! ",
                            $"That's a great thing to ask about, {userName}! ",
                            $"Excellent question, {userName}! "
                        });
                        break;
                    case "frustrated":
                        openings.AddRange(new[]
                        {
                            $"I can see this is frustrating, {userName}. ",
                            $"Let me help simplify this for you, {userName}. ",
                            $"Don't worry, {userName}, I'm here to make this clearer. "
                        });
                        break;
                    default:
                        openings.AddRange(new[]
                        {
                            $"{userName}, here's what you should know: ",
                            $"Sure, {userName}! ",
                            $"Great, {userName}! "
                        });
                        break;
                }
            }

            // Add transition phrases for context awareness
            if (context != null && context.IsTopicContinuation(nlpResult?.Intent ?? ""))
            {
                openings = openings.Select(o => o.Replace("here's", "building on that, here's")).ToList();
            }

            return openings[random.Next(openings.Count)];
        }

        /// <summary>
        /// Generates a natural closing phrase for responses
        /// </summary>
        private string GenerateNaturalClosing(string sentiment, bool isFollowUp, NLPResult nlpResult)
        {
            var closings = new List<string>();

            if (isFollowUp)
            {
                closings.AddRange(new[]
                {
                    " Feel free to ask if you'd like to know more!",
                    " Is there anything specific you'd like to explore further?",
                    " Let me know if you have any other questions!"
                });
            }
            else
            {
                switch (sentiment)
                {
                    case "worried":
                        closings.AddRange(new[]
                        {
                            " Remember, taking these steps will help keep you safe online!",
                            " You're taking the right steps by learning about this!",
                            " Don't hesitate to ask if you need more guidance!"
                        });
                        break;
                    case "curious":
                        closings.AddRange(new[]
                        {
                            " Feel free to ask if you want to dive deeper!",
                            " I'm here if you have more questions!",
                            " Let me know what else you'd like to learn about!"
                        });
                        break;
                    case "frustrated":
                        closings.AddRange(new[]
                        {
                            " Take it one step at a time—you've got this!",
                            " I'm here to help make this easier for you!",
                            " Don't hesitate to ask if anything is still unclear!"
                        });
                        break;
                    default:
                        closings.AddRange(new[]
                        {
                            " Feel free to ask if you need more information!",
                            " Let me know if you have any other questions!",
                            " I'm here to help with anything else you'd like to know!"
                        });
                        break;
                }
            }

            return closings[random.Next(closings.Count)];
        }

        /// <summary>
        /// Resets the current topic
        /// </summary>
        public void ResetTopic()
        {
            currentTopic = null;
        }

        /// <summary>
        /// Sets the current topic
        /// </summary>
        public void SetTopic(string topic)
        {
            currentTopic = topic;
        }
    }
}

