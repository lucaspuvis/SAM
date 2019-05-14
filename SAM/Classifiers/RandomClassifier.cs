using SAM.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;


namespace SAM.Classifiers
{
    /// <summary>
    /// A random classifier which randomly evaluates a comment.
    /// </summary>
    class RandomClassifier : IEvaluator
    {
        Random Rand = new Random();
        
        /// <summary>
        /// Randomly evaluates a comment.
        /// </summary>
        /// <param name="comment">The comment to be randomly evaluated.</param>
        /// <returns>A tuple containing the random evaluation.</returns>
        public (string, float) EvaluateComment(IEnumerable<string> comment)
        {
            return Evaluation(Rand.Next(-1, 2));
        }

        /// <summary>
        /// Randomly evaluates a sentence.
        /// </summary>
        /// <param name="sentence">The sentence to be randomly evaluated.</param>
        /// <returns>A tuple containing the random evaluation.</returns>
        public (string, float) EvaluateSentence(string sentence)
        {
            return Evaluation(Rand.Next(-1, 2));
        }

        /// <summary>
        /// Randomly evaluates the individual sentences of a comment.
        /// </summary>
        /// <param name="comment">The comment which sentences has to be randomly evaluated.</param>
        /// <returns>A list containing the random evaluations.</returns>
        public IEnumerable<(string, float)> EvaluateSentencesInComment(IEnumerable<string> comment)
        {
            foreach (var _ in comment)
                yield return Evaluation(Rand.Next(-1, 2));
        }

        /// <summary>
        /// Constructs an evaluation based on the sentiment.
        /// </summary>
        /// <param name="sentiment">The sentiment to make the evaluation upon.</param>
        /// <returns>A tuple containing an evaluation.</returns>
        public (string, float) Evaluation(float sentiment)
        {
            if (sentiment < -0.1) return (Program.InternConcurrentSafe("Negative"), sentiment);
            else if (sentiment > 0.1) return (Program.InternConcurrentSafe("Positive"), sentiment);
            return (Program.InternConcurrentSafe("Neutral"), sentiment);
        }
    }
}
