using System.Collections.Generic;
using ProtoBuf;
using RazzleServer.DataProvider.References;

namespace RazzleServer.DataProvider.Cache
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class CachedQuizzes
    {
        public Dictionary<int, QuizReference> Data { get; set; } = new Dictionary<int, QuizReference>();
    }
}
