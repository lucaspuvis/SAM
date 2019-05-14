using System.Collections.Generic;

namespace SAM.Interfaces
{
    public interface IEvaluator
    {
        /// <summary>
        /// Predicts and returns the rating and label.
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns>label, rating</returns>
        (string, float) EvaluateSentence(string sentence);

        /// <summary>
        /// Evaluates each sentence in a comment individually
        /// </summary>
        /// <param name="comment"></param>
        /// <returns>IEnumerable of label,ratings </returns>
        IEnumerable<(string, float)> EvaluateSentencesInComment(IEnumerable<string> comment);

        /// <summary>
        /// Take a rating and determine the label
        /// </summary>
        /// <param name="sentiment"></param>
        /// <returns>label, rating</returns>
        (string, float) Evaluation(float sentiment);

        /// <summary>
        /// Evaluates the whole comment as one
        /// </summary>
        /// <param name="comment"></param>
        /// <returns>label, rating</returns>
        (string, float) EvaluateComment(IEnumerable<string> comment);

    }
}