using System;
using System.Collections.Generic;

namespace QuizApp.Models
{
    public class Question
    {
        public string ImagePath { get; set; }
        public string Answer { get; set; }
    }

    public class Theme
    {
        public string Title { get; set; }
        public List<Question> Questions { get; set; } = new List<Question>();
    }

    public class QuizData
    {
        public List<Theme> Themes { get; set; } = new List<Theme>();
    }
}