using System.Collections.Generic;

namespace RazzleServer.DataProvider.References
{
    public class QuizReference
    {
        public int Id { get; set; }

        public List<QuizQuestionReference> Questions { get; set; } = new List<QuizQuestionReference>();
    }
}
