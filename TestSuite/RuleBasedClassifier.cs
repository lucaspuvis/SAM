using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using SAM;
using SAM.PlaceHolders;
using SAM.Interfaces;

namespace TestSuite
{
    [TestClass]
    public class RuleBasedTests
    {
        public Dictionary<string, int> Lexicon;
        public Dictionary<string, List<IModifier>> Trigger;
        public Tokenizer Tokenizer;
        public RuleBasedClassifier RuleBased;
        public string PositiveSentence;
        public string NeutralSentence;
        public string NegativeSentence;
        public string OneWordLookahead;
        public string TwoWordLookahead;
        public string VendingSentence;

        [TestInitialize]
        public void Initialize()
        {
            Lexicon = new Dictionary<string, int>() {
                { "hejsa", 1 },
                { "dig", 1 },
                { "nedern", -1 },
                { "sq", -1 },
                { "godt", 1 } };

            Trigger = new Dictionary<string, List<IModifier>>() {
                { "sq", new List<IModifier>(){ new Mult(2f,1)} } };
            
            PositiveSentence = "hejsa med digsa.";
            NegativeSentence = "nedern er det.";
            NeutralSentence = "et sort hul.";
            OneWordLookahead = "det var meget godt.";
            TwoWordLookahead = "det var ikke så godt.";
            VendingSentence = "hun har hovedet under armen.";

            var setting = new Setting();
            Tokenizer = new Tokenizer(setting);
            RuleBased = new RuleBasedClassifier(Lexicon, setting, Trigger);
        }

        [TestMethod]
        public void RuleBased_given_single_positive_sentence_returns_sentiment()
        {   
            var (evaluation, rating) = RuleBased.EvaluateSentence(PositiveSentence);

            Assert.AreEqual("Positive", evaluation);
            Assert.AreEqual(1, rating);
        }

        [TestMethod]
        public void RuleBased_given_single_negative_sentence_returns_sentiment()
        {
            var (evaluation, rating) = RuleBased.EvaluateSentence(NegativeSentence);

            Assert.AreEqual("Negative", evaluation);
            Assert.AreEqual(-1, rating);
        }

        [TestMethod]
        public void RuleBased_given_single_neutral_sentence_returns_sentiment()
        {
            var (evaluation, rating) = RuleBased.EvaluateSentence(NeutralSentence);

            Assert.AreEqual("Neutral", evaluation);
            Assert.AreEqual(0, rating);
        }
        
        [TestMethod]
        public void RuleBased_given_one_sentence_in_comment_returns_sentiment()
        {
            var comment = new List<string>() { PositiveSentence };
            var (evaluation, rating) = RuleBased.EvaluateComment(comment);

            Assert.AreEqual("Positive", evaluation);
            Assert.AreEqual(1, rating);
        }

        [TestMethod]
        public void RuleBased_given_two_sentences_returns_sentiment()
        {
            var comment = new List<string>() { PositiveSentence, NegativeSentence };
            var evaluation = (List<(string, float)>)RuleBased.EvaluateSentencesInComment(comment);

            Assert.AreEqual("Positive", evaluation[0].Item1);
            Assert.AreEqual(1, evaluation[0].Item2);
            Assert.AreEqual("Negative", evaluation[1].Item1);
            Assert.AreEqual(-1, evaluation[1].Item2);
        }

        [TestMethod]
        public void RuleBased_given_one_comment_returns_sentiment()
        {
            var comment = new List<string>() { PositiveSentence, NegativeSentence, NeutralSentence };
            var evaluation = (List<(string, float)>)RuleBased.EvaluateSentencesInComment(comment);

            Assert.AreEqual("Positive", evaluation[0].Item1);
            Assert.AreEqual(1, evaluation[0].Item2);
            Assert.AreEqual("Negative", evaluation[1].Item1);
            Assert.AreEqual(-1, evaluation[1].Item2);
            Assert.AreEqual("Neutral", evaluation[2].Item1);
            Assert.AreEqual(0, evaluation[2].Item2);
        }

        [TestMethod]
        public void RuleBased_given_one_word_lookahead_sentence_returns_sentiment()
        { 
            var (evaluation, estimate) = RuleBased.EvaluateSentence(OneWordLookahead);

            Assert.AreEqual("Positive", evaluation);
            Assert.AreEqual(1, estimate);
        }

        [TestMethod]
        public void RuleBased_given_two_word_lookahead_sentence_returns_sentiment()
        {
            var (evaluation, estimate) = RuleBased.EvaluateSentence(TwoWordLookahead);

            Assert.AreEqual("Positive", evaluation);
            Assert.AreEqual(1, estimate);
        }

        [TestMethod]
        public void RuleBased_given_vending_in_sentence_returns_sentiment() {
            var (evaluation, estimate) = RuleBased.EvaluateSentence(VendingSentence);

            Assert.AreEqual("Neutral", evaluation);
            Assert.AreEqual(0, estimate);
        }
    }
}
