using System;
using System.Collections.Generic;

namespace Final_Version.Services
{
    /// <summary>
    /// Tracks conversation context for better understanding and more natural responses
    /// </summary>
    public class ConversationContext
    {
        private readonly Queue<ConversationTurn> recentTurns;
        private const int MaxHistorySize = 5;
        
        public string CurrentTopic { get; set; }
        public string LastIntent { get; set; }
        public DateTime LastInteraction { get; set; }
        public int TurnCount { get; private set; }

        public ConversationContext()
        {
            recentTurns = new Queue<ConversationTurn>();
            LastInteraction = DateTime.Now;
        }

        /// <summary>
        /// Adds a turn to the conversation history
        /// </summary>
        public void AddTurn(string userInput, string botResponse, string intent)
        {
            var turn = new ConversationTurn
            {
                UserInput = userInput,
                BotResponse = botResponse,
                Intent = intent,
                Timestamp = DateTime.Now
            };

            recentTurns.Enqueue(turn);
            if (recentTurns.Count > MaxHistorySize)
            {
                recentTurns.Dequeue();
            }

            LastIntent = intent;
            LastInteraction = DateTime.Now;
            TurnCount++;
        }

        /// <summary>
        /// Gets recent conversation history
        /// </summary>
        public List<ConversationTurn> GetRecentHistory(int count = 3)
        {
            var history = new List<ConversationTurn>(recentTurns);
            if (history.Count > count)
            {
                history = history.GetRange(history.Count - count, count);
            }
            return history;
        }

        /// <summary>
        /// Checks if this is a continuation of previous topic
        /// </summary>
        public bool IsTopicContinuation(string newIntent)
        {
            if (recentTurns.Count == 0)
                return false;

            var lastTurn = recentTurns.Peek();
            return lastTurn.Intent == newIntent || 
                   (lastTurn.Intent == "ask_about" && newIntent == "follow_up");
        }

        /// <summary>
        /// Resets the conversation context
        /// </summary>
        public void Reset()
        {
            recentTurns.Clear();
            CurrentTopic = null;
            LastIntent = null;
            TurnCount = 0;
            LastInteraction = DateTime.Now;
        }

        /// <summary>
        /// Gets time since last interaction
        /// </summary>
        public TimeSpan TimeSinceLastInteraction()
        {
            return DateTime.Now - LastInteraction;
        }
    }

    /// <summary>
    /// Represents a single turn in the conversation
    /// </summary>
    public class ConversationTurn
    {
        public string UserInput { get; set; }
        public string BotResponse { get; set; }
        public string Intent { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

