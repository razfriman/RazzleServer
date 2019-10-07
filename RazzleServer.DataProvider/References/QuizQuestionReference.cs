using ProtoBuf;

namespace RazzleServer.DataProvider.References
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuizQuestionReference
    {
        public int Id { get; set; }

        public string Question { get; set; }

        public bool Answer { get; set; }

        public string Response { get; set; }
    }
}
