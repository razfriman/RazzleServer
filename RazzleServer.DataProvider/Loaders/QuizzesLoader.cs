using RazzleServer.DataProvider.Cache;
using RazzleServer.DataProvider.References;
using RazzleServer.Wz;
using Serilog;

namespace RazzleServer.DataProvider.Loaders
{
    public sealed class QuizzesLoader : ACachedDataLoader<CachedQuizzes>
    {
        public override string CacheName => "Quizzes";

        public override ILogger Logger => Log.ForContext<QuizzesLoader>();

        public override void LoadFromWz(WzFile file)
        {
            Logger.Information("Loading Quizzes");
            
            var dir = file.WzDirectory.GetDirectoryByName("Etc");
            var img = dir.GetImageByName("OXQuiz.img");
            img.WzProperties.ForEach(quizImg =>
            {
                if (!int.TryParse(quizImg.Name, out var quizId))
                {
                    return;
                }

                var quiz = new QuizReference {Id = quizId};

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
                        Id = questionId, Question = question, Answer = answer, Response = response
                    });
                });

                Data.Data.Add(quiz.Id, quiz);
            });
        }
    }
}
