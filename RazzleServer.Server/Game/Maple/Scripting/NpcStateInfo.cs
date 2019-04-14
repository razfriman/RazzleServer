using System.Collections.Generic;
using RazzleServer.Common.Constants;

namespace RazzleServer.Game.Maple.Scripting
{
    public class NpcStateInfo
    {
        public NpcMessageType Type { get; set; }

        public string Text { get; set; }

        public bool IsPrevious { get; set; }

        public bool IsNext { get; set; }

        public List<int> Styles { get; set; } = new List<int>();

        public int NumberDefault { get; set; }

        public int NumberMinimum { get; set; }

        public int NumberMaximum { get; set; }

        public NpcQuizType NpcQuizType { get; set; }

        public int QuizObjectId { get; set; }

        public int QuizCorrect { get; set; }

        public int QuizQuestions { get; set; }

        public int QuizTime { get; set; }
    }
}
