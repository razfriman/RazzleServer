using System.Collections.Generic;
using RazzleServer.DataProvider.References;

namespace RazzleServer.DataProvider.Cache
{
    public sealed class CachedQuizzes
    {
        public Dictionary<int, QuizReference> Data { get; set; } = new Dictionary<int, QuizReference>();
    }
}
