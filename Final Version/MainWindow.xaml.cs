using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Final_Version
{
    public partial class MainWindow : Window
    {
        private Bot bot; 

        public MainWindow()
        {
            InitializeComponent();
            InitializeBot();
            string soundPath = "C:\\Users\\uthac\\OneDrive\\Documents\\programing\\C#\\Final Version\\Final Version\\Resources\\Message.wav";
            PlaySound(soundPath);
            string imagePath = "C:\\Users\\uthac\\OneDrive\\Documents\\programing\\C#\\Final Version\\Final Version\\Resources\\BotImage.bmp";
            RenderAsciiArt(imagePath);
        }

        private void InitializeBot()
        {
            bot = new Bot();
            bot.OnResponseGenerated += Bot_OnResponseGenerated;

            // Set default user details
            bot.SetUserDetails(NameTextBox.Text, FavoriteTopicTextBox.Text);
            UpdateChat("Welcome to the Cybersecurity Chat Bot! Enter your name and favorite topic, then ask about topics like 'passwords', 'scams', 'privacy', or general terms like 'cybersecurity', 'firewall', or 'malware'. You can also manage tasks with 'add task - [title]: [description]', 'view tasks', 'delete task -[index]', or 'complete task -[index]', or start a quiz with 'start quiz'.");
        }

        private void SetDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            bot.SetUserDetails(NameTextBox.Text, FavoriteTopicTextBox.Text);
            UpdateChat($"User details updated: Name = {NameTextBox.Text}, Favorite Topic = {FavoriteTopicTextBox.Text}");
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessInput();
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessInput();
            }
        }

        private void ProcessInput()
        {
            if (!string.IsNullOrWhiteSpace(InputTextBox.Text))
            {
                string input = InputTextBox.Text;
                UpdateChat($"You: {input}");
                bot.ProcessInput(input);
                InputTextBox.Clear();
                InputTextBox.Focus(); // Keep focus on input for follow-ups
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ChatTextBlock.Text = "";
            InitializeBot();
            UpdateChat("Conversation reset. Welcome back!");

                    // Replay sound on reset
        string soundPath = "C:\\Users\\uthac\\OneDrive\\Documents\\programing\\C#\\Final Version\\Final Version\\Resources\\Message.wav";
        PlaySound(soundPath);

        // Re-render ASCII art on reset
        string imagePath = "C:\\Users\\uthac\\OneDrive\\Documents\\programing\\C#\\Final Version\\Final Version\\Resources\\BotImage.bmp";
        RenderAsciiArt(imagePath);
        }


        private void Bot_OnResponseGenerated(bool recognized, string response)
        {
            UpdateChat(response);
            if (response.Contains("Would you like a reminder? (e.g., 'yes in 3 days', 'yes tomorrow', 'yes 2025-06-30 12:00', or 'no')"))
            {
                // Prompt user for yes/no with timeframe response
                InputTextBox.Focus();
            }
        }

        private void UpdateChat(string message)
        {
            ChatTextBlock.Text += (ChatTextBlock.Text.Length > 0 ? "\n\n" : "") + message;
        }

        private void PlaySound(string soundPath)
        {
            try
            {
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(soundPath);
                player.Load();
                player.Play();
            }catch(Exception ex)
            {
                ChatTextBlock.Text += $"Error playing sound {ex.Message}\n";
            }
        }

        private void RenderAsciiArt(string imagePath)
        {
            try
            {
                if (!File.Exists(imagePath))
                {
                    ChatTextBlock.Text += "Error: Image file not found at " + imagePath + "\n";
                    return;
                }

                using (Bitmap image = new Bitmap(imagePath))
                {
                    string asciiChars = "@#%*+=-:. ";
                    int newWidth = 60;
                    int newHeight = (int)(image.Height / (double)image.Width * newWidth * 0.55);
                    Bitmap resizedImage = new Bitmap(image, new System.Drawing.Size(newWidth, newHeight));

                    StringBuilder asciiArt = new StringBuilder();
                    for (int y = 0; y < resizedImage.Height; y++)
                    {
                        for (int x = 0; x < resizedImage.Width; x++)
                        {
                            Color pixelColor = resizedImage.GetPixel(x, y);
                            int gray = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                            int charIndex = (gray * (asciiChars.Length - 1)) / 255;
                            asciiArt.Append(asciiChars[charIndex]);
                        }
                        asciiArt.Append("\n");
                    }

                    ChatTextBlock.Text += asciiArt.ToString();
                }
            }catch(Exception ex)
            {
                ChatTextBlock.Text += $"Error rendering ASCII art: {ex.Message}\n";
            }
        }
    }
}