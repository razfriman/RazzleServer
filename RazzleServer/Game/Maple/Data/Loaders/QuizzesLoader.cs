using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Data.Cache;
using RazzleServer.Game.Maple.Data.References;

namespace RazzleServer.Game.Maple.Data.Loaders
{
    public sealed class QuizzesLoader : ACachedDataLoader<CachedQuizzes>
    {
        public override string CacheName => "Quizzes";

        private readonly ILogger _log = LogManager.Log;

        public override void LoadFromWz()
        {
            _log.LogInformation("Loading Quizs");

            using (var file = GetWzFile("Etc.wz"))
            {
                file.ParseWzFile();
                var img = file.WzDirectory.GetImageByName("OXQuiz.img");
                img.WzProperties.ForEach(quizImg =>
                {
                    if (!int.TryParse(quizImg.Name, out var quizId))
                    {
                        return;
                    }

                    var quiz = new QuizReference
                    {
                        Id = quizId
                    };

                    quizImg.WzProperties.ForEach(questionImg =>
                    {
                        if (!int.TryParse(questionImg.Name, out var questionId))
                        {
                            return;
                        }


                        var question = questionImg["q"]?.GetString();
                        var answer = (questionImg["a"]?.GetInt() ?? 0) > 0;
                        var response = questionImg["d"]?.GetString();
                        quiz.Questions.Add(new QuizQuestionReference
                        {
                            Id = questionId,
                            Question = question,
                            Answer = answer,
                            Response = response
                        });

                    });

                    Data.Data.Add(quiz.Id, quiz);

                });
            }
        }
    }
}