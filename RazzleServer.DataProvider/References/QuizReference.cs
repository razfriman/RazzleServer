using System.Collections.Generic;
using ProtoBuf;

namespace RazzleServer.DataProvider.References
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuizReference
    {
        public int Id { get; set; }

        public List<QuizQuestionReference> Questions { get; set; } = new List<QuizQuestionReference>();
    }
}
