namespace Final_Version.Models
{
    /// <summary>
    /// Represents a quiz question with options, correct answer, and explanation
    /// </summary>
    public class QuizQuestion
    {
        public string Question { get; set; }
        public string[] Options { get; set; }
        public int CorrectAnswerIndex { get; set; }
        public string Explanation { get; set; }
    }
}

