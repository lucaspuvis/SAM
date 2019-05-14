using System;
using System.Collections.Generic;
using System.Linq;
using SAM.Interfaces;
using SAM.PlaceHolders;

namespace SAM.Classifiers
{
    /// <summary>
    /// The class for the Rule Based Classifier.
    /// This classifier determines sentence naively
    /// using a lexicon of words and one for 'triggerwords'.
    /// </summary>
    public class RuleBasedClassifier : IEvaluator
    {
        private readonly IDictionary<string, int> Lexicon;
        private readonly IDictionary<string, List<IModifier>> TriggerLex;
        private readonly Setting settings;
        private int id;

        /// <summary>
        /// The constructor for the RuleBasedClassifier.
        /// </summary>
        /// <param name="Lex">A Dictionary containing words and ratings for those words.</param>
        /// <param name="settings">A settings object for behavioural changes.</param>
        /// <param name="triggerlex">A Dictionary containing 'triggerwords' and 'vendinger'.</param>
        public RuleBasedClassifier(IDictionary<string, int> Lex, Setting settings, IDictionary<string, List<IModifier>> triggerlex)
        {
            Lexicon = Lex;
            TriggerLex = triggerlex;
            this.settings = settings;
        }

        /// <summary>
        /// This function takes a string, tokenizes it and then determines the 
        /// sentiment based on the given lexicon.
        /// </summary>
        /// <param name="sentence">The sentence of type string to be evaluated.</param>
        /// <returns>A tuple containing the estimated sentiment and rating.</returns>
        public (string, float) EvaluateSentence(string sentence)
        {
            var tsentence = CreateTSentence(sentence);
            var tokens = tsentence.tokens.ToArray();
            float sentiment = 0;
            for (int i = 0; i < tokens.Count(); i++)
            {
                if (TriggerLex.TryGetValue(tokens[i].Word, out List<IModifier> modifiers))
                    foreach (Modifier mod in modifiers) mod.ModifySentiment(ref tokens, i);

                sentiment += tokens[i].Sentiment;
            }
            return Evaluation(sentiment);
        }
        
        /// <summary>
        /// This function takes an IEnumerable of sentences
        /// and evaluates each sentence in a comment individually.
        /// </summary>
        /// <param name="comment">An IEnumerable of sentences of type string.</param>
        /// <returns>A list containing all individual evaluations.</returns>
        public IEnumerable<(string, float)> EvaluateSentencesInComment(IEnumerable<string> comment)
        {
            var list = new List<(string, float)>();
            foreach (var sentence in comment)
            {
                list.Add(EvaluateSentence(sentence));
            }
            return list;
        }

        /// <summary>
        /// This function takes an IEnumerable of sentences and evaluates 
        /// them together as one comment.
        /// </summary>
        /// <param name="comment">The IEnumerable of sentences of type string.</param>
        /// <returns>A tuple containing the estimated sentiment and rating.</returns>
        public (string, float) EvaluateComment(IEnumerable<string> comment)
        {
            float sentiment = 0;
            foreach(var sentence in comment)
            {
                sentiment += EvaluateSentence(sentence).Item2;
            }
            return Evaluation(sentiment);
        }

        /// <summary>
        /// This function determines an evaluation of a given sentiment rating.
        /// </summary>
        /// <param name="sentiment">The sentiment rating to be evaluated.</param>
        /// <returns>A tuple containing the Evaluation and the sentiment rating.</returns>
        public (string, float) Evaluation(float sentiment)
        {
            if (sentiment > 0) return (Program.InternConcurrentSafe("Positive"), sentiment);
            if (sentiment < 0) return (Program.InternConcurrentSafe("Negative"), sentiment);
            else return (Program.InternConcurrentSafe("Neutral"), sentiment);
        }

        /// <summary>
        /// This function takes a sentence, tokenizes it, and gives each token sentiment
        /// based on the given lexicon.
        /// </summary>
        /// <param name="sentence">The sentence to be created as a string.</param>
        /// <returns>A tokenized sentence of type TSentence.</returns>
        private TSentence CreateTSentence(string sentence)
        {
            Tokenizer tokenizer = new Tokenizer(settings);
            var tsentence = tokenizer.TokenizeSentence(sentence, id++);
            foreach (var token in tsentence) {
                if (Lexicon.TryGetValue(token.Word, out int val))
                {
                    token.Sentiment = val;
                }
                else if (settings.writeNonMatchTokens) FileWriter.WriteNonMatched(token);
            }
            return tsentence;
        }
    }


}
