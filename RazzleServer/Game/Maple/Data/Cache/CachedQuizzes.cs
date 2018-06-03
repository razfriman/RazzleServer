using System.Collections.Generic;
using RazzleServer.Game.Maple.Data.References;

namespace RazzleServer.Game.Maple.Data.Cache
{
    public sealed class CachedQuizzes
    {
        public Dictionary<int, QuizReference> Data { get; set; } = new Dictionary<int, QuizReference>();
    }
}
