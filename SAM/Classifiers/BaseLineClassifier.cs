using System;
using System.Collections.Generic;
using System.Text;
using SAM.Interfaces;

namespace SAM.Classifiers
{
    /// <summary>
    /// Classifier which will always predict neutral.
    /// </summary>
    class BaselineClassifer : IEvaluator
    {
        /// <summary>
        /// Evaluates a comment.
        /// </summary>
        /// <param name="comment">The comment to be evaluated.</param>
        /// <returns>A tuple containing a neutral evaluation.</returns>
        public (string, float) EvaluateComment(IEnumerable<string> comment)
        {
            return Evaluation(0.0f);
        }

        /// <summary>
        /// Evaluates a sentence.
        /// </summary>
        /// <param name="sentence">The sentence to be evaluated.</param>
        /// <returns>A tuple containing a neutral evaluation.</returns>
        public (string, float) EvaluateSentence(string sentence)
        {
            return Evaluation(0.0f);
        }

        /// <summary>
        /// Evaluates each sentence individually in a comment.
        /// </summary>
        /// <param name="comment">The comment which sentences has to be evaluated.</param>
        /// <returns>A list containing neutral evaluations.</returns>
        public IEnumerable<(string, float)> EvaluateSentencesInComment(IEnumerable<string> comment)
        {
            foreach (var _ in comment)
                yield return Evaluation(0.0f);
        }

        /// <summary>
        /// Constructs an evaluation based on the sentiment.
        /// </summary>
        /// <param name="sentiment">The sentiment to make the evaluation upon.</param>
        /// <returns>A tuple containing an evaluation.</returns>
        public (string, float) Evaluation(float sentiment)
        {
            if (sentiment < -0.1) return (Program.InternConcurrentSafe("Negative"), sentiment);
            if (sentiment > 0.1) return (Program.InternConcurrentSafe("Positive"), sentiment);
            else return (Program.InternConcurrentSafe("Neutral"), sentiment);
        }
    }
}
