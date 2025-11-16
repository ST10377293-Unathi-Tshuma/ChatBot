[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/Q0H1VfQX)

# Cybersecurity Chatbot

## Overview
This is a C# console-based chatbot designed to educate users about cybersecurity topics, including password safety, phishing scams, and safe browsing practices. It provides interactive features such as a quiz, task management with reminders, and activity logging. The chatbot uses basic natural language processing (NLP) to understand user inputs and respond with relevant information, adapting responses based on detected user sentiment.

The code is structured within the `Final_Version` namespace and implements a `Bot` class that handles user interactions, topic-based responses, and task management.

---

## Features

- **Cybersecurity Education**: Advice on password safety, phishing scams, and privacy.
- **Interactive Quiz**: A 10-question quiz with explanations and scoring.
- **Task Management**: Add, complete, delete, and set reminders for tasks.
- **Activity Logging**: Tracks all key interactions and actions.
- **Sentiment Detection**: Responds empathetically based on detected emotions.
- **Topic-Based Conversations**: Handles user queries by topic, supports follow-up prompts.
- **Favorite Topic**: Remembers and prioritizes user-defined topics of interest.

---

## Requirements

- **.NET Framework / .NET Core**  
- **IDE**: Visual Studio or VS Code with C# support  
- No third-party dependencies required

---

## Installation

1. Clone or download the repository.
2. Open the project in Visual Studio or VS Code.
3. Ensure the appropriate .NET SDK is installed.
4. Build the solution.

---

## Usage

### 1. Run the Program  
Compile and run the bot using your IDE or:
```bash
dotnet run
````

### 2. Set User Details

Use:

```csharp
bot.SetUserDetails("Alice", "passwords");
```

### 3. Interact with the Bot

Use natural language commands:

* General:
  `Tell me about passwords`
  `What is a firewall?`
  `I’m worried about scams`
* Follow-up:
  `Tell me more`
  `Explain`
* Quiz:
  `start quiz`
* Tasks:
  `add task review privacy settings`
  `remind me to update password`
  `yes in 3 days`
  `complete task review privacy settings`
  `delete task update password`
* View Logs/Tasks:
  `view log`
  `view tasks`

### 4. Capture Bot Events

Example:

```csharp
bot.OnResponseGenerated += (recognized, response) => Console.WriteLine(response);
```

---

## Code Structure

### Namespace: `Final_Version`

### Main Class: `Bot`

#### Delegates

* `ResponseHandler`
* `SentimentDetectedHandler`
* `TopicChangedHandler`

#### Key Methods

* `SetUserDetails(string name, string favTopic)`
* `ProcessInput(string input)`
* `StartQuiz()`
* `AddTaskFromInput(string input)`
* `ViewTasks()`
* `ViewLog()`
* `DetectSentiment(string input)`
* `AdjustResponse(string baseResponse, string sentiment, bool isFollowUp)`

#### Data Structures

* `topicResponses`: Dictionary for topic content
* `generalKnowledge`: Dictionary of simple cybersecurity facts
* `keywordToTopic`: Maps user keywords to topic identifiers
* `quizQuestions`: List of `QuizQuestion` objects
* `tasks`: List of `TaskItem` objects
* `activityLog`: List of string-formatted time-stamped events

---

## Example Interaction

```
Bot: Hello, Alice! What would you like to learn about today? Try asking about passwords, scams, or privacy.

User: Tell me about passwords  
Bot: Alice, here’s some info: Always use strong, unique passwords...  

User: Tell me more  
Bot: Great, let’s dive deeper into Password Safety, Alice!...

User: I’m worried about scams  
Bot: Alice, it’s completely understandable to feel that way. Phishing scams trick you...

User: start quiz  
Bot: Alice, here’s question 1 of 10: What is the minimum recommended length for a strong password?  
1. 6 characters  
2. 8 characters  
3. 12 characters  
4. 16 characters  
Please answer with the number (1–4) of your choice.

User: 3  
Bot: Correct, Alice! Your current score: 1/1.
```

---

## Limitations

* **NLP**: Simple, keyword-based NLP; lacks deep intent understanding.
* **Reminders**: Stored but not actively triggered—requires polling or scheduled checks.
* **Sentiment**: Limited to keywords like "worried", "curious", "frustrated".
* **Quiz**: Fixed 10 questions, no variation or randomization.
* **UI**: Console only; needs integration for GUI or web interface.

---

## Future Improvements

* Use NuGet NLP libraries (e.g., ML.NET or LUIS).
* Implement active reminder notifications.
* Expand quiz pool and introduce randomness.
* Add GUI (e.g., WinForms, WPF, ASP.NET).
* Support more user intents with better NLP parsing.

---

## Acknowledgments

Matlock, T. (2023). *Exploring Natural Language Processing with C#: From Basics to Advanced Techniques*. [Medium](https://medium.com/@thomasmatlockbba/exploring-natural-language-processing-with-c-from-basics-to-advanced-techniques-9114f7da802a) \[Accessed 26 Jun. 2025].
