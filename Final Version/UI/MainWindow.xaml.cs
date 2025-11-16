using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Drawing = System.Drawing;
using Final_Version.Core;
using static Final_Version.Constants;

namespace Final_Version.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Bot bot;

        public MainWindow()
        {
            InitializeComponent();
            InitializeBot();
            PlayWelcomeSound();
            RenderAsciiArt();
        }

        private string placeholderText = "What is cybersecurity?";
        private bool isPlaceholderActive = true;

        /// <summary>
        /// Initializes the bot and sets up event handlers
        /// </summary>
        private void InitializeBot()
        {
            bot = new Bot();
            bot.OnResponseGenerated += Bot_OnResponseGenerated;

            // Set default user details
            bot.SetUserDetails(NameTextBox.Text, FavoriteTopicTextBox.Text);
            UpdateChat(WelcomeMessage, isBotMessage: true);
            
            // Send initial greeting to start conversation
            SendInitialGreeting();
        }

        /// <summary>
        /// Sends an initial greeting to start the conversation
        /// </summary>
        private void SendInitialGreeting()
        {
            // Simulate user greeting to get bot to respond
            string greeting = "Hello";
            UpdateChat($"You: {greeting}", isBotMessage: false);
            bot.ProcessInput(greeting);
        }

        /// <summary>
        /// Handles the Set Details button click
        /// </summary>
        private void SetDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            bot.SetUserDetails(NameTextBox.Text, FavoriteTopicTextBox.Text);
            UpdateChat($"✓ User details updated: Name = {NameTextBox.Text}, Favorite Topic = {FavoriteTopicTextBox.Text}", isBotMessage: true);
        }

        /// <summary>
        /// Handles the Submit button click
        /// </summary>
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessInput();
        }

        /// <summary>
        /// Handles Enter key press in the input textbox
        /// </summary>
        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessInput();
            }
        }

        /// <summary>
        /// Processes user input and sends it to the bot
        /// </summary>
        private void ProcessInput()
        {
            string input = InputTextBox.Text.Trim();
            
            // Don't process placeholder text
            if (isPlaceholderActive || string.IsNullOrWhiteSpace(input) || input == placeholderText)
            {
                InputTextBox.Text = placeholderText;
                InputTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF"));
                isPlaceholderActive = true;
                return;
            }

            UpdateChat($"You: {input}", isBotMessage: false);
            bot.ProcessInput(input);
            
            // Reset to placeholder
            InputTextBox.Text = placeholderText;
            InputTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF"));
            isPlaceholderActive = true;
            InputTextBox.Focus(); // Keep focus on input for follow-ups
        }

        /// <summary>
        /// Handles input textbox got focus event
        /// </summary>
        private void InputTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (isPlaceholderActive)
            {
                InputTextBox.Text = "";
                InputTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F2937"));
                isPlaceholderActive = false;
            }
        }

        /// <summary>
        /// Handles input textbox lost focus event
        /// </summary>
        private void InputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputTextBox.Text))
            {
                InputTextBox.Text = placeholderText;
                InputTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF"));
                isPlaceholderActive = true;
            }
        }

        /// <summary>
        /// Handles quick action button clicks
        /// </summary>
        private void QuickAction_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button)
            {
                string query = button.Content.ToString();
                
                // Remove emoji if present
                if (query.Contains("💡"))
                {
                    query = query.Replace("💡", "").Trim();
                }
                
                // Set input and process
                InputTextBox.Text = query;
                InputTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F2937"));
                isPlaceholderActive = false;
                ProcessInput();
            }
        }

        /// <summary>
        /// Handles the Reset button click
        /// </summary>
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ChatTextBlock.Text = "";
            InitializeBot();
            UpdateChat("🔄 Conversation reset. Welcome back!", isBotMessage: true);

            // Reset input placeholder
            InputTextBox.Text = placeholderText;
            InputTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF"));
            isPlaceholderActive = true;

            // Replay sound on reset
            PlayWelcomeSound();

            // Re-render ASCII art on reset
            RenderAsciiArt();
        }

        /// <summary>
        /// Handles bot response generation events
        /// </summary>
        private void Bot_OnResponseGenerated(bool recognized, string response)
        {
            UpdateChat(response, isBotMessage: true, isRecognized: recognized);
            if (response.Contains("Would you like a reminder?"))
            {
                // Prompt user for yes/no with timeframe response
                InputTextBox.Focus();
            }
        }

        /// <summary>
        /// Updates the chat display with a new message
        /// </summary>
        private void UpdateChat(string message, bool isBotMessage = true, bool isRecognized = true)
        {
            Dispatcher.Invoke(() =>
            {
                if (ChatTextBlock.Text.Length > 0)
                {
                    ChatTextBlock.Text += "\n\n";
                }

                // Add emoji prefix based on message type
                if (isBotMessage)
                {
                    if (isRecognized)
                    {
                        ChatTextBlock.Text += "🤖 Bot: ";
                    }
                    else
                    {
                        ChatTextBlock.Text += "❓ Bot: ";
                    }
                }

                ChatTextBlock.Text += message;

                // Auto-scroll to bottom
                ScrollToBottom();
            });
        }

        /// <summary>
        /// Scrolls the chat to the bottom
        /// </summary>
        private void ScrollToBottom()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                ChatScrollViewer.ScrollToEnd();
            }));
        }

        /// <summary>
        /// Plays the welcome sound from resources
        /// </summary>
        private void PlayWelcomeSound()
        {
            try
            {
                string soundPath = GetResourcePath(SoundFileName);
                if (File.Exists(soundPath))
                {
                    System.Media.SoundPlayer player = new System.Media.SoundPlayer(soundPath);
                    player.Load();
                    player.Play();
                }
            }
            catch (Exception ex)
            {
                UpdateChat($"⚠️ Error playing sound: {ex.Message}", isBotMessage: true);
            }
        }

        /// <summary>
        /// Renders ASCII art from the bot image
        /// </summary>
        private void RenderAsciiArt()
        {
            try
            {
                string imagePath = GetResourcePath(ImageFileName);
                if (!File.Exists(imagePath))
                {
                    UpdateChat($"⚠️ Error: Image file not found at {imagePath}", isBotMessage: true);
                    return;
                }

                using (Drawing.Bitmap image = new Drawing.Bitmap(imagePath))
                {
                    int newWidth = AsciiArtWidth;
                    int newHeight = (int)(image.Height / (double)image.Width * newWidth * AsciiArtHeightRatio);
                    Drawing.Bitmap resizedImage = new Drawing.Bitmap(image, new Drawing.Size(newWidth, newHeight));

                    StringBuilder asciiArt = new StringBuilder();
                    asciiArt.Append("```\n");
                    for (int y = 0; y < resizedImage.Height; y++)
                    {
                        for (int x = 0; x < resizedImage.Width; x++)
                        {
                            Drawing.Color pixelColor = resizedImage.GetPixel(x, y);
                            int gray = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                            int charIndex = (gray * (AsciiChars.Length - 1)) / 255;
                            asciiArt.Append(AsciiChars[charIndex]);
                        }
                        asciiArt.Append("\n");
                    }
                    asciiArt.Append("```\n");

                    ChatTextBlock.Text += asciiArt.ToString();
                    ScrollToBottom();
                }
            }
            catch (Exception ex)
            {
                UpdateChat($"⚠️ Error rendering ASCII art: {ex.Message}", isBotMessage: true);
            }
        }

        /// <summary>
        /// Gets the full path to a resource file
        /// </summary>
        private string GetResourcePath(string fileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDirectory, ResourcesFolder, fileName);
        }
    }
}
